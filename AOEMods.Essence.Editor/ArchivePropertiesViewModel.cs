using AOEMods.Essence.SGA.Graph;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace AOEMods.Essence.Editor;

public class ArchivePropertiesViewModel : ObservableRecipient
{
    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private string? name = null;

    public string? SignatureString
    {
        get => signatureString;
        set => SetProperty(ref signatureString, value);
    }

    private string? signatureString = null;

    public string? TocName
    {
        get => tocName;
        set => SetProperty(ref tocName, value);
    }

    private string? tocName = null;

    public string? TocAlias
    {
        get => tocAlias;
        set => SetProperty(ref tocAlias, value);
    }

    private string? tocAlias = null;

    public IArchive? Archive
    {
        get => archive;
        set => SetProperty(ref archive, value);
    }

    private IArchive? archive;

    public ICommand ApplyCommand { get; }

    public ArchivePropertiesViewModel()
    {
        ApplyCommand = new RelayCommand(Apply);
    }

    private void Apply()
    {
        if (Archive != null)
        {
            if (SignatureString == null || Name == null)
            {
                throw new InvalidOperationException("Tried to apply archive properties but archive was not set.");
            }

            Archive.Name = Name;
            Archive.Signature = SignatureString
                .Split(' ')
                .Select(byteString => byte.Parse(byteString, NumberStyles.HexNumber))
                .ToArray();
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Archive))
        {
            if (Archive != null)
            {
                Name = Archive.Name;
                SignatureString = string.Join(" ", Archive.Signature.Select(signatureByte => signatureByte.ToString("X2")));
                TocName = Archive.Tocs[0].Name;
                TocAlias = Archive.Tocs[0].Alias;
            }
            else
            {
                Name = null;
                SignatureString = null;
                TocName = null;
                TocAlias = null;
            }
        }
    }
}
