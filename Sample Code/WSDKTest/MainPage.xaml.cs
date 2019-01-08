using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using DJI.WindowsSDK;
using Windows.UI.Popups;
using System.Reflection;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace WSDKTest
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DJIVideoParser.Parser videoParser;
        public WriteableBitmap VideoSource;

        //Worker task (thread) for reading barcode
        //As reading barcode is computationally expensive
        private Task readerWorker = null;

        private object bufLock = new object();
        //these properties are guarded by bufLock
        private int width, height;
        private byte[] decodedDataBuf;

        public MainPage()
        {
            this.InitializeComponent();
            //Listen for registration success
            DJISDKManager.Instance.SDKRegistrationStateChanged += async (state, result) =>
            {
                if (state != SDKRegistrationState.Succeeded)
                {
                    var md = new MessageDialog(result.ToString());
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async ()=> await md.ShowAsync());
                    return;
                }
                //wait till initialization finish
                //use a large enough time and hope for the best
                await Task.Delay(1000);
                videoParser = new DJIVideoParser.Parser();
                videoParser.Initialize();
                videoParser.SetVideoDataCallack(0, 0, ReceiveDecodedData);                
                DJISDKManager.Instance.VideoFeeder.GetPrimaryVideoFeed(0).VideoDataUpdated += OnVideoPush;
            };
            DJISDKManager.Instance.RegisterApp("Your ID");

        }

        void OnVideoPush(VideoFeed sender, [ReadOnlyArray] ref byte[] bytes)
        {
            this.videoParser.PushVideoData(0, 0, bytes, bytes.Length);
        }

        void createWorker()
        {
            //create worker thread for reading barcode
            readerWorker = new Task(async () =>
            {
                BarcodeReader reader = new BarcodeReader()
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options =
                    {
                        TryHarder = true,
                        ReturnCodabarStartEnd = true,
                        PureBarcode = false
                    }
                };
                //use stopwatch to time the execution, and execute the reading process repeatedly
                var watch = System.Diagnostics.Stopwatch.StartNew();
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    //local cache of databuf, width and height
                    //has to copy in a lock as they might change during execution
                    byte[] databuf = null;
                    uint width = 0, height = 0;
                    while (true)
                    {
                        watch.Restart();
                        lock(bufLock)
                        {
                            databuf = (byte[])decodedDataBuf.Clone();
                            width   = (uint)this.width;
                            height  = (uint)this.height;
                        }
                        //crop the image to the region bounded by the red rectangle
                        //basically the middle portion of the image
                        //copied and pasted from stackoverflow, I wish to be able to reuse the encoder and decoder,
                        //but there are state errors when I try to do so, we need to have faith on the GC...
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                            (uint)width, (uint)height, 72, 72, databuf);
                        encoder.BitmapTransform.Bounds = new BitmapBounds()
                        {
                            X = width/ 4,
                            Y = height / 4,
                            Width = width / 2,
                            Height = height / 2
                        };
                        await encoder.FlushAsync();
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                        //decode the cropped image
                        var r = reader.Decode(await decoder.GetSoftwareBitmapAsync(
                            BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore));

                        //use dispatcher to run the UI update in UI thread
                        //I forgot how to use XAML binding, whatever
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            result.Text = "None";
                            if (r != null)
                                result.Text = r.Text;
                        });
                        
                        watch.Stop();
                        int elapsed = (int)watch.ElapsedMilliseconds;
                        //run at max 5Hz
                        await Task.Delay(Math.Max(0, 200 - elapsed));
                    }
                }
            });
        }

        async void ReceiveDecodedData(byte[] data, int width, int height)
        {
            //basically copied from the sample code
            lock (bufLock)
            {
                //lock when updating decoded buffer, as this is run in async
                //some operation in this function might overlap, so operations involving buffer, width and height must be locked
                if (decodedDataBuf == null)
                {
                    decodedDataBuf = data;
                }
                else
                {
                    if (data.Length != decodedDataBuf.Length)
                    {
                        Array.Resize(ref decodedDataBuf, data.Length);
                    }
                    data.CopyTo(decodedDataBuf.AsBuffer());
                    this.width = width;
                    this.height = height;
                }
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //dispatch to UI thread to do UI update (image)
                //WriteableBitmap is exclusive to UI thread
                if (VideoSource == null || VideoSource.PixelWidth != width || VideoSource.PixelHeight !=  height)
                {
                    VideoSource = new WriteableBitmap((int)width, (int)height);
                    fpvImage.Source = VideoSource;
                    //Start barcode reader worker after the first frame is received
                    if (readerWorker == null)
                    {
                        createWorker();
                        readerWorker.Start();
                    }
                }
                lock (bufLock)
                {
                    //copy buffer to the bitmap and draw the region we will read on to notify the users
                    decodedDataBuf.AsBuffer().CopyTo(VideoSource.PixelBuffer);
                    using (VideoSource.GetBitmapContext())
                    {
                        VideoSource.DrawRectangle(width / 4, height / 4, width * 3 / 4, height * 3 / 4, Windows.UI.Colors.Red);
                    }
                }
                //Invalidate cache and trigger redraw
                VideoSource.Invalidate();
            });
        }
    }
}
