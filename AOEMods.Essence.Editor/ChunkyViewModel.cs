using AOEMods.Essence.Chunky;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace AOEMods.Essence.Editor
{
    public class ChunkyViewModel : TabItemViewModel
    {
        public ObservableCollection<ChunkyNodeViewModel>? RootChildren
        {
            get => rootChildren;
            set => SetProperty(ref rootChildren, value);
        }

        private ObservableCollection<ChunkyNodeViewModel>? rootChildren = null;

        public ChunkyFile? ChunkyFile
        {
            get => chunkyFile;
            set => SetProperty(ref chunkyFile, value);
        }

        private ChunkyFile? chunkyFile = null;

        public Stream? DataStream
        {
            get => dataStream;
            set => SetProperty(ref dataStream, value);
        }

        private Stream? dataStream = null;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(ChunkyFile))
            {
                if (chunkyFile != null)
                {
                    RootChildren = new ObservableCollection<ChunkyNodeViewModel>(
                        chunkyFile.RootNodes.Select(
                            rootNode => new ChunkyNodeViewModel()
                            {
                                Node = rootNode
                            }
                        )
                    );
                }
                else
                {
                    RootChildren = null;
                }
            }
        }
    }
}
