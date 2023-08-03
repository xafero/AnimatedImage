using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedImageAvalonia.Demo.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private ImageSource _selectedSource;
        public ImageSource SelectedSource
        {
            get => _selectedSource;
            set => this.RaiseAndSetIfChanged(ref _selectedSource, value);
        }

        private ObservableCollection<ImageSource> _sources;
        public ObservableCollection<ImageSource> Sources
        {
            get => _sources;
            set => this.RaiseAndSetIfChanged(ref _sources, value);
        }

        public MainWindowViewModel()
        {
            Sources = new(new[] {
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/bomb-once.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/Bomb.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/earth.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/monster.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/nonanimated.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/nonanimated.png")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/partialfirstframe.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/pause.png")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/play.png")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/radar.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/siteoforigin.gif")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/UnsupportImageFormat.bmp")),
                new ImageSource(new Uri("avares://AnimatedImageAvalonia.Demo/Assets/working.gif"))
            });


            this.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedSource))
            {
                SelectedSourceUpdated();
            }
        }

        private void SelectedSourceUpdated()
        {


        }
    }

    public class ImageSource
    {
        public string Name { get; }
        public Uri Source { get; }

        public ImageSource(Uri source)
        {
            Name = source.ToString();
            Source = source;
        }

        public override string ToString() => Name;
    }
}
