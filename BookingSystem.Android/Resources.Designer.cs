﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BookingSystem.Android {
    using System;
    using System.Reflection;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("BookingSystem.Android.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static string APIBaseAddress {
            get {
                return ResourceManager.GetString("APIBaseAddress", resourceCulture);
            }
        }
        
        internal static string APIBaseAddressRelease {
            get {
                return ResourceManager.GetString("APIBaseAddressRelease", resourceCulture);
            }
        }
        
        internal static string APIKey {
            get {
                return ResourceManager.GetString("APIKey", resourceCulture);
            }
        }
        
        internal static string ERR_MSG_CONNECTION {
            get {
                return ResourceManager.GetString("ERR_MSG_CONNECTION", resourceCulture);
            }
        }
        
        internal static string ERR_MSG_INVALID_ACTIVATION_CODE {
            get {
                return ResourceManager.GetString("ERR_MSG_INVALID_ACTIVATION_CODE", resourceCulture);
            }
        }
        
        internal static string ERR_MSG_SERVER_ERROR {
            get {
                return ResourceManager.GetString("ERR_MSG_SERVER_ERROR", resourceCulture);
            }
        }
        
        internal static string ERR_MSG_TIMEOUT {
            get {
                return ResourceManager.GetString("ERR_MSG_TIMEOUT", resourceCulture);
            }
        }
    }
}
