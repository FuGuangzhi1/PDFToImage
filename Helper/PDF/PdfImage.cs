using PDFiumCore;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using WebApplication1.Helper.PDF;


public unsafe class PdfImage : IDisposable
{
    private readonly FpdfBitmapT _pdfBitmap;
    private readonly UnmanagedMemoryManager<byte> _mgr;

    public int Width { get; }

    public int Height { get; }

    public int Stride { get; }

    public Image<Bgra32> ImageData { get; }

    internal PdfImage(
        FpdfBitmapT pdfBitmap,
        int width,
        int height)
    {
        _pdfBitmap = pdfBitmap;
        nint scan0 = fpdfview.FPDFBitmapGetBuffer(pdfBitmap);
        Stride = fpdfview.FPDFBitmapGetStride(pdfBitmap);
        Height = height;
        Width = width;
        _mgr = new UnmanagedMemoryManager<byte>((byte*)scan0, Stride * Height);

        ImageData = Image.WrapMemory<Bgra32>(Configuration.Default, _mgr.Memory, width, height);
    }

    public void Dispose()
    {
        ImageData.Dispose();
        fpdfview.FPDFBitmapDestroy(_pdfBitmap);
    }
}