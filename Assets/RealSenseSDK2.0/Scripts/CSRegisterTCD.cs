using System;
using System.Runtime.InteropServices;

namespace CSRegister
{
    public struct cRegImages
    {
        public IntPtr wThermal;
        public IntPtr wThermalCM;
        public IntPtr wThermalOverColor;
    }

    class CSRegisterTCD : IDisposable
    {
        #region PInvokes
        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern IntPtr CreateRegisterClass();

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern IntPtr CreateRegisterClassByFile(string file_path, bool invertRT);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern void DisposeRegisterClass(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern int call_getThermalWidth(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern int call_getThermalHeight(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern int call_getColorDepthWidth(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern int call_getColorDepthHeight(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern IntPtr call_readRGBimage(IntPtr pCreateRegisterObject, string path);

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern IntPtr call_readGRAYimage(IntPtr pCreateRegisterObject, string path);

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern void call_writeRGBimage(IntPtr pCreateRegisterObject, IntPtr pimg, string filename, int width, int height);

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern void call_writeRGBAimage(IntPtr pCreateRegisterObject, IntPtr pimg, string filename, int width, int height);

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern void call_writeGRAYimage(IntPtr pCreateRegisterObject, IntPtr pimg, string filename, int width, int height);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern void call_update(IntPtr pCreateRegisterObject, IntPtr t_img, IntPtr c_img, IntPtr d_img,
            bool registerImag, float temp_min, float temp_max, float opacity, out int width, out int height/*,
            ref IntPtr warpedThermal, ref IntPtr warpedThermalCM, ref IntPtr thermoOverColor*/);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern IntPtr call_getWarpedThermal(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern IntPtr call_getWarpedThermalCM(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll")]
        static private extern IntPtr call_getThermoOverColor(IntPtr pCreateRegisterObject);

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern void call_show3cImg(IntPtr pCreateRegisterObject, string name, IntPtr img, int width, int height, int waitKey);

        [DllImport("lib_register_tcd_cpp.dll", CharSet = CharSet.Ansi)]
        static private extern void call_removeShow(IntPtr pCreateRegisterObject, string name, bool all);
        #endregion PInvokes

        #region Members
        private IntPtr m_pNativeObject; // Variable to hold c++ class
        #endregion Members

        public CSRegisterTCD()
        {
            this.m_pNativeObject = CreateRegisterClass();
        }

        public CSRegisterTCD(string file_path, bool invertRT)
        {
            this.m_pNativeObject = CreateRegisterClassByFile(file_path, invertRT);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool bDisposing)
        {
            if (this.m_pNativeObject != IntPtr.Zero)
            {
                // Call the DLL Export to dispose this class
                DisposeRegisterClass(this.m_pNativeObject);
                this.m_pNativeObject = IntPtr.Zero;
            }

            if (bDisposing)
            {
                // No need to call the finalizer since we've now cleaned
                // up the unmanaged memory
                GC.SuppressFinalize(this);
            }
        }

        // This finalizer is called when Garbage collection occurs, but only if
        // the IDisposable.Dispose method wasn't already called.
        ~CSRegisterTCD()
        {
            Dispose(false);
        }

        #region Wrapper methods
        public int getThermalWidth()
        {
            return call_getThermalWidth(this.m_pNativeObject);
        }

        public int getThermalHeight()
        {
            return call_getThermalHeight(this.m_pNativeObject);
        }

        public int getColorDepthWidth()
        {
            return call_getColorDepthWidth(this.m_pNativeObject);
        }

        public int getColorDepthHeight()
        {
            return call_getColorDepthHeight(this.m_pNativeObject);
        }

        public IntPtr readRGBimage(string path)
        {
            return call_readRGBimage(this.m_pNativeObject, path);
        }

        public IntPtr readGRAYimage(string path)
        {
            return call_readGRAYimage(this.m_pNativeObject, path);
        }

        public void writeRGBimage(IntPtr pimg, string filename, int width, int height)
        {
            if (!(pimg == IntPtr.Zero))
            {
                call_writeRGBimage(this.m_pNativeObject, pimg, filename, width, height);
            }
            else
            {
                Console.WriteLine("ERROR.writeRGBimage: data is void.");
            }
        }

        public void writeRGBAimage(IntPtr pimg, string filename, int width, int height)
        {
            if (!(pimg == IntPtr.Zero))
            {
                call_writeRGBAimage(this.m_pNativeObject, pimg, filename, width, height);
            }
            else
            {
                Console.WriteLine("ERROR.writeRGBAimage: data is void.");
            }
        }

        public void writeGRAYimage(IntPtr pimg, string filename, int width, int height)
        {
            if (!(pimg == IntPtr.Zero))
            {
                call_writeGRAYimage(this.m_pNativeObject, pimg, filename, width, height);
            }
            else
            {
                Console.WriteLine("ERROR.writeGRAYimage: data is void.");
            }
        }

        public void update(IntPtr t_img, IntPtr c_img, IntPtr d_img, bool registerImag,
            float temp_min, float temp_max, float opacity, out int width, out int height/*,
            ref IntPtr warpedThermal, ref IntPtr warpedThermalCM, ref IntPtr thermoOverColor*/)
        {
           call_update(this.m_pNativeObject, t_img, c_img, d_img, registerImag, temp_min, temp_max, opacity, out width, out height/*,
               ref warpedThermal, ref warpedThermalCM, ref thermoOverColor*/);

            //if(warpedThermal == IntPtr.Zero)
            //{
            //    Console.WriteLine("ERROR.update: warpedThermal is void.");
            //}
            //if (warpedThermalCM == IntPtr.Zero)
            //{
            //    Console.WriteLine("ERROR.update: warpedThermalCM is void.");
            //}
            //if (thermoOverColor == IntPtr.Zero)
            //{
            //    Console.WriteLine("ERROR.update: thermoOverColor is void.");
            //}

        }
        
        public IntPtr getWarpedThermal()
        {
            return call_getWarpedThermal(this.m_pNativeObject);
        }
        
        public IntPtr getWarpedThermalCM()
        {
            return call_getWarpedThermalCM(this.m_pNativeObject);
        }
        
        public IntPtr getThermoOverColor()
        {
            return call_getThermoOverColor(this.m_pNativeObject);
        }

        public void show3cImg(string name, IntPtr img, int width, int height, int waitKey)
        {
            call_show3cImg(this.m_pNativeObject, name, img, width, height, waitKey);
        }

        public void removeShow(string name, bool all)
        {
            call_removeShow(this.m_pNativeObject, name, all);
        }

        #endregion Wrapper methods

    }
}
