using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FontAutoScalingMaui
{
    public partial class ViewModel : ObservableObject
    {
        public ObservableCollection<Model> Models { get; set; }

        public ViewModel() 
        {
            Models = new()
            {
                new Model {Name = "John", Description = "Some description"},
                new Model {Name = "John", Description = "Some description"},
                new Model {Name = "John", Description = "Some description"},
                new Model {Name = "John", Description = "Some description"}
            };
        }
    }
}
