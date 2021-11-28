using AOEMods.Essence.Chunky.RRTex;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AOEMods.Essence.Editor;

public class TextureViewModel : TabItemViewModel
{
    public BitmapImage? Image
    {
        get => image;
        set => SetProperty(ref image, value);
    }

    private BitmapImage? image = null;

    public TextureMip? ImageFile
    {
        get => imageFile;
        set => SetProperty(ref imageFile, value);
    }

    private TextureMip? imageFile = null;

    public ICommand ExportCommand { get; }

    public TextureViewModel()
    {
        ExportCommand = new RelayCommand(Export);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ImageFile))
        {
            if (ImageFile.HasValue)
            {
                var im = new BitmapImage();
                im.BeginInit();
                im.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                im.CacheOption = BitmapCacheOption.OnLoad;
                im.UriSource = null;
                im.StreamSource = new MemoryStream(ImageFile.Value.Data);
                im.EndInit();
                im.Freeze();
                Image = im;
            }
            else
            {
                Image = null;
            }
        }
    }

    private void Export()
    {
        if (Image != null)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = $"png (*.png)|*.png|jpg (*.jpg)|*jpg|bmp (*.bmp)|*.bmp|tiff (*.tiff)|*.tiff|wmp (*.wmp)|*.wmp|gif (*.gif)|*.gif|All files (*.*)|*.*",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var extension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                BitmapEncoder encoder = extension switch
                {
                    ".png" => new PngBitmapEncoder(),
                    ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
                    ".bmp" => new BmpBitmapEncoder(),
                    ".tiff" => new TiffBitmapEncoder(),
                    ".wmp" => new WmpBitmapEncoder(),
                    ".gif" => new GifBitmapEncoder(),
                    _ => throw new NotImplementedException($"No encoder for extension {extension}")
                };
                encoder.Frames.Add(BitmapFrame.Create(Image));
                using var fileStream = File.OpenWrite(saveFileDialog.FileName);
                encoder.Save(fileStream);
            }
        }
    }
}
