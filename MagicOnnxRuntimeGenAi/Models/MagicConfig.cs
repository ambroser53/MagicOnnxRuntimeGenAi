using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace MagicOnnxRuntimeGenAi
{
    public class MagicConfig : IDisposable
    {
        private IntPtr _configHandle;
        private bool _disposed = false;
        public HardwareType hardwareType { get; set; }
        public string modelPath { get; set; }
        private MagicNativeMethods _MagicNativeMethods;
        public MagicConfig(string _modelPath)
        {
            modelPath = _modelPath;
            hardwareType = GetHardwareType();
            _MagicNativeMethods = new MagicNativeMethods(hardwareType);
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaCreateConfig(MagicStringUtils.ToUtf8(_modelPath), out _configHandle));
        }

        internal IntPtr Handle { get { return _configHandle; } }
        public void ClearProviders()
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaConfigClearProviders(_configHandle));
        }

        public void AppendProvider(string provider)
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaConfigAppendProvider(_configHandle, MagicStringUtils.ToUtf8(provider)));
        }

        public void SetProviderOption(string provider, string option, string value)
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaConfigSetProviderOption(_configHandle, MagicStringUtils.ToUtf8(provider), MagicStringUtils.ToUtf8(option), MagicStringUtils.ToUtf8(value)));
        }

        ~MagicConfig()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _MagicNativeMethods.OgaDestroyConfig(_configHandle);
            _configHandle = IntPtr.Zero;
            _disposed = true;
        }
        
        public HardwareType GetHardwareType()
        {
            // Path to the genai_config.json file
            string filePath = Path.Combine(modelPath, "genai_config.json");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The genai_config.json file was not found in the directory.");
            }

            // Read the file content
            string jsonContent = File.ReadAllText(filePath);

            // Parse the JSON content using Newtonsoft.Json.Linq
            JObject json = JObject.Parse(jsonContent);

            // Extract the provider options
            var providerOptions = json["model"]?["decoder"]?["session_options"]?["provider_options"];

            // Determine the hardware type
            if (providerOptions != null && providerOptions.HasValues)
            {
                var firstProviderOption = providerOptions.First as JObject;

                if (firstProviderOption.ContainsKey("dml"))
                {
                    return HardwareType.dml;
                }
                else if (firstProviderOption.ContainsKey("cuda"))
                {
                    return HardwareType.cuda;
                }
            }

            // If provider_options is empty or null, assume CPU
            return HardwareType.cpu;
        }
    }
}