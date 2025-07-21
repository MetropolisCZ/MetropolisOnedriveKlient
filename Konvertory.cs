using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MetropolisOnedriveKlient
{

    public class KonvertorRepozitarIkonaSlozkyNeboSouboru : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            FolderTrida item = value as FolderTrida;
            if (item != null)
            {
                return "\uF12B";
            }
            else
            {
                return "\uE7C3";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class KonvertorViditelnostiSdileno : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Shared sdileno = value as Shared;
            if (sdileno == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }




    /*public class KonvertorViditelnostThumbnailu : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var thumbnails = value as List<ThumbnailSet>;
            if (thumbnails.Count != 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class KonvertorViditelnostThumbnailuInvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var thumbnails = value as List<ThumbnailSet>;
            if (thumbnails.Count == 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }*/



    public class KonvertorBnaMB : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            long velikostB = (long)value;


            double megabytes = velikostB / (1024.0 * 1024.0);
            return $"{megabytes:F2} MB"; // Format to 2 decimal places
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }




    public class VybratIkonuDleObsahu : DataTemplateSelector
    {
        public DataTemplate SablonaSouboryRepozitarGithub_IkonaSlozky { get; set; }
        public DataTemplate SablonaSouboryRepozitarGithub_IkonaGenericka { get; set; }
        public DataTemplate SablonaSouboryRepozitarGithub_IkonaObrazkovyNahled { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            OneDriveAdresarSoubory jedenSouborOneDrive = item as OneDriveAdresarSoubory;

            if (jedenSouborOneDrive?.Thumbnails?.FirstOrDefault()?.Small?.Url != null)
            {
                return SablonaSouboryRepozitarGithub_IkonaObrazkovyNahled;
            }
            else if (jedenSouborOneDrive?.Folder != null)
            {
                return SablonaSouboryRepozitarGithub_IkonaSlozky;
            }
            else
            {
                return SablonaSouboryRepozitarGithub_IkonaGenericka;
            }

        }
    }
}
