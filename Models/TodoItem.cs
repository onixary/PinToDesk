using System;
using System.ComponentModel;

namespace PinToDesk.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private int _order = 0;

        public Guid Id { get; set; } = Guid.NewGuid();

        private TodoColor _color;
        private TodoColor? _previewColor;
        public TodoColor Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(DisplayColor)); }
        }

        internal TodoColor? PreviewColor
        {
            set { _previewColor = value; OnPropertyChanged(nameof(DisplayColor)); }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public TodoColor DisplayColor => _previewColor ?? Color;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int Order
        {
            get => _order;
            set
            {
                if (_order != value)
                {
                    _order = value;
                    OnPropertyChanged(nameof(Order));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
