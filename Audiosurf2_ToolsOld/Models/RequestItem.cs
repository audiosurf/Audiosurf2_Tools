using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Audiosurf2_Tools.Models
{
    public class RequestItem : ReactiveObject
    {
        private string _title;
        private string _channel;
        private TimeSpan _duration;

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public string Channel
        {
            get => _channel;
            set => this.RaiseAndSetIfChanged(ref _channel, value);
        }

        public TimeSpan Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public RequestItem(string title, string channel, TimeSpan duration)
        {
            _title = title;
            _channel = channel;
            _duration = duration;
        }
    }
}
