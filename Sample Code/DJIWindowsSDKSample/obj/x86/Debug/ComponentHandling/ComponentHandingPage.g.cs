﻿#pragma checksum "C:\Users\eason\Desktop\ws\WSDK-FPV-HelloWorld\Sample Code\DJIWindowsSDKSample\ComponentHandling\ComponentHandingPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2C60F78E0DB8DD58A4882BBC0F5D7420"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DJIWindowsSDKSample.ComponentHandling
{
    partial class ComponentHandingPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // ComponentHandling\ComponentHandingPage.xaml line 11
                {
                    this.RootGrid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3: // ComponentHandling\ComponentHandingPage.xaml line 22
                {
                    this.eventSamplesPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 4: // ComponentHandling\ComponentHandingPage.xaml line 45
                {
                    this.settingSamplesPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 5: // ComponentHandling\ComponentHandingPage.xaml line 55
                {
                    this.gettingSamplePanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 6: // ComponentHandling\ComponentHandingPage.xaml line 66
                {
                    this.actionSamplesPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}
