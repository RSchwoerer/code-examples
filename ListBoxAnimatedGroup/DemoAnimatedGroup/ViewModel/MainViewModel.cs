using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace DemoAnimatedGroup.ViewModel
{
    public class MainViewModel
    {
        private readonly ObservableCollection<CityViewModel> cities = new ObservableCollection<CityViewModel>();

        public ObservableCollection<CityViewModel> Cities
        {
            get { return this.cities; }
        }

        public MainViewModel()
        {
            var xDocument = XDocument.Load("Data/Cities.xml");
            var query = from c in xDocument.Descendants("City")
                        select new CityViewModel
                                   {
                                       Name = c.Attribute("Name").Value,
                                       Population = long.Parse(c.Attribute("Population").Value),
                                       Country = c.Attribute("Country").Value
                                   };

            foreach (var cityViewModel in query)
            {
                this.Cities.Add(cityViewModel);
            }
        }
    }
}
