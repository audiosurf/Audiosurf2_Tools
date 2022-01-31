using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Audiosurf2_Tools.Models
{
    public class LocalPlaylistItem : ReactiveObject, IPlaylistItem
    {
        private string _title = "";
        private string _artist = "";
        private TimeSpan _duration;
        private string _location = "";
        private Bitmap _coverImage;

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Artist
        {
            get => _artist;
            set => this.RaiseAndSetIfChanged(ref _artist, value);
        }

        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public string Location
        {
            get => _location;
            set => this.RaiseAndSetIfChanged(ref _location, value);
        }

        public Bitmap CoverImage
        {
            get => _coverImage;
            set => this.RaiseAndSetIfChanged(ref _coverImage, value);
        }

        public bool Loaded { get; set; } = false;

        public LocalPlaylistItem(string location)
        {
            this.Location = location;
        }

        public Task<bool> LoadInfoAsync()
        {
            using (var file = TagLib.File.Create(Location))
            { ;
                Title = file.Tag.Title ?? Location.Split("\\").Last();
                Artist = file.Tag.FirstPerformer;
                Duration = file.Properties.Duration;
                if (file.Tag.Pictures.Length != 0)
                {
                    using (var ms = new MemoryStream(file.Tag.Pictures.FirstOrDefault()!.Data.Data))
                    {
                        ms.Position = 0;
                        CoverImage = new Bitmap(ms);
                    }
                }
            }
            this.Loaded = true;
            return Task.FromResult(true);
        }
    }
}
