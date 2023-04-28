namespace MCNBTEditor.Core.Services {
    public interface IClipboardService {
        string ReadableText { get; set; }

        byte[] BinaryData { get; set; }

        byte[] GetBinaryTag(string format);
        void SetBinaryTag(string format, byte[] data);
    }
}