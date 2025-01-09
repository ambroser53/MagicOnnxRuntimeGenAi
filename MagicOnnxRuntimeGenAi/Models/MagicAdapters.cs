using System.Runtime.InteropServices;
using System;

namespace MagicOnnxRuntimeGenAi
{
    /// <summary>
    /// A container of adapters.
    /// </summary>
    public class MagicAdapters : SafeHandle
    {
        private MagicNativeMethods _MagicNativeMethods;
        
        /// <summary>
        /// Creates a container for adapters
        /// used to load, unload and hold them.
        /// Throws on error.
        /// </summary>
        /// <param name="model">Reference to a loaded model</param>
        /// <returns>new Adapters object</returns>
        public MagicAdapters(MagicModel model) : base(IntPtr.Zero, true)
        {
            _MagicNativeMethods = model.GetMagicNativeMethods();
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaCreateAdapters(model.Handle, out handle));
        }

        /// <summary>
        /// Method that loads adapter data and assigns it a nmae that
        /// it can be referred to. Throws on error.
        /// </summary>
        /// <param name="adapterPath">file path to load</param>
        /// <param name="adapterName">adapter name</param>
        public void LoadAdapter(string adapterPath, string adapterName)
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaLoadAdapter(handle,
                MagicStringUtils.ToUtf8(adapterPath), MagicStringUtils.ToUtf8(adapterName)));
        }

        /// <summary>
        /// Unload the adatper that was loaded by the LoadAdapter method.
        /// Throws on error.
        /// </summary>
        /// <param name="adapterName"></param>
        public void UnloadAdapter(string adapterName)
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaUnloadAdapter(handle, 
                MagicStringUtils.ToUtf8(adapterName)));
        }

        internal IntPtr Handle { get { return handle; } }

        /// <summary>
        /// Implement SafeHandle override
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            _MagicNativeMethods.OgaDestroyAdapters(handle);
            handle = IntPtr.Zero;
            return true;
        }
    }
}