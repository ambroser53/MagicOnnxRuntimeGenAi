using System;

namespace MagicOnnxRuntimeGenAi
{
    public class MagicGenerator : IDisposable
    {
        private IntPtr _generatorHandle;

        private bool _disposed;
        private MagicNativeMethods _MagicNativeMethods;
        public MagicGenerator(MagicModel model, MagicGeneratorParams generatorParams)
        {
            _MagicNativeMethods = model.GetMagicNativeMethods();
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaCreateGenerator(model.Handle, generatorParams.Handle, out _generatorHandle));
        }
        public bool IsDone()
        {
            return _MagicNativeMethods.OgaGenerator_IsDone(_generatorHandle) != 0;
        }

        public void AppendTokens(ReadOnlySpan<int> tokens)
        {
            unsafe
            {
                fixed (int* tokenIDsPtr = tokens)
                {
                    new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaGenerator_AppendTokens(_generatorHandle, tokenIDsPtr, (UIntPtr)tokens.Length));
                }
            }
        }

        public void AppendTokenSequences(MagicSequences sequences)
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaGenerator_AppendTokenSequences(_generatorHandle, sequences.Handle));
        }

        public void RewindTo(ulong index)
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaGenerator_RewindTo(_generatorHandle, (UIntPtr)index));
        }

        public void GenerateNextToken()
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaGenerator_GenerateNextToken(_generatorHandle));
        }

        public unsafe ReadOnlySpan<int> GetSequence(ulong index)
        {
            ulong num = _MagicNativeMethods.OgaGenerator_GetSequenceCount(_generatorHandle, (nuint)index).ToUInt64();
            return new ReadOnlySpan<int>(_MagicNativeMethods.OgaGenerator_GetSequenceData(_generatorHandle, (nuint)index).ToPointer(), (int)num);
        }
        
        public void SetActiveAdapter(MagicAdapters adapters, string adapterName)
        {
            new MagicResult(_MagicNativeMethods).VerifySuccess(_MagicNativeMethods.OgaSetActiveAdapter(_generatorHandle,
                adapters.Handle,
                MagicStringUtils.ToUtf8(adapterName)));
        }

        ~MagicGenerator()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _MagicNativeMethods.OgaDestroyGenerator(_generatorHandle);
                _generatorHandle = IntPtr.Zero;
                _disposed = true;
            }
        }
    }
}