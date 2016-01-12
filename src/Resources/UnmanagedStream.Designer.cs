﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace skwas.IO.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UnmanagedStream {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UnmanagedStream() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("skwas.IO.Resources.UnmanagedStream", typeof(UnmanagedStream).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stream does not support reading..
        /// </summary>
        internal static string Argument_StreamNotReadable {
            get {
                return ResourceManager.GetString("Argument_StreamNotReadable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stream does not support seeking..
        /// </summary>
        internal static string Argument_StreamNotSeekable {
            get {
                return ResourceManager.GetString("Argument_StreamNotSeekable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stream does not support writing..
        /// </summary>
        internal static string Argument_StreamNotWriteable {
            get {
                return ResourceManager.GetString("Argument_StreamNotWriteable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The underlying stream has been released..
        /// </summary>
        internal static string InvalidOperationException_StreamReleased {
            get {
                return ResourceManager.GetString("InvalidOperationException_StreamReleased", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stream can&apos;t be flushed..
        /// </summary>
        internal static string IOException_StreamCantFlush {
            get {
                return ResourceManager.GetString("IOException_StreamCantFlush", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to get length of stream..
        /// </summary>
        internal static string IOException_StreamGetLength {
            get {
                return ResourceManager.GetString("IOException_StreamGetLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stream is not initialized or has been released..
        /// </summary>
        internal static string IOException_StreamNotInitialized {
            get {
                return ResourceManager.GetString("IOException_StreamNotInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to read from stream..
        /// </summary>
        internal static string IOException_StreamRead {
            get {
                return ResourceManager.GetString("IOException_StreamRead", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to seek in stream..
        /// </summary>
        internal static string IOException_StreamSeek {
            get {
                return ResourceManager.GetString("IOException_StreamSeek", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to set size of stream..
        /// </summary>
        internal static string IOException_StreamSetLength {
            get {
                return ResourceManager.GetString("IOException_StreamSetLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to write to stream..
        /// </summary>
        internal static string IOException_StreamWrite {
            get {
                return ResourceManager.GetString("IOException_StreamWrite", resourceCulture);
            }
        }
    }
}
