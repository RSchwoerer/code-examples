Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Data

Public Class ScrollbarOnFarLeftConverter
   Implements IValueConverter

   Public Function Convert( _
               ByVal value As Object, _
               ByVal targetType As System.Type, _
               ByVal parameter As Object, _
               ByVal culture As System.Globalization.CultureInfo _
            ) As Object Implements IValueConverter.Convert
      If value Is Nothing Then Return False
      Return (DirectCast(value, Double) > 0)
   End Function

   Public Function ConvertBack( _
               ByVal value As Object, _
               ByVal targetType As System.Type, _
               ByVal parameter As Object, _
               ByVal culture As System.Globalization.CultureInfo _
            ) As Object Implements IValueConverter.ConvertBack
      Throw New System.NotImplementedException()
   End Function

End Class

Public Class ScrollbarOnFarRightConverter
   Implements IMultiValueConverter

   Public Function Convert( _
               ByVal values As Object(), _
               ByVal targetType As System.Type, _
               ByVal parameter As Object, _
               ByVal culture As System.Globalization.CultureInfo _
            ) As Object Implements IMultiValueConverter.Convert
      If values Is Nothing Then Return False
      If values(0) Is Nothing OrElse values(1) Is Nothing OrElse values(2) Is Nothing Then Return False
      If values(0).Equals(Double.NaN) OrElse values(1).Equals(Double.NaN) OrElse values(2).Equals(Double.NaN) Then Return False

      Dim dblHorizontalOffset As Double = 0
      Dim dblViewportWidth As Double = 0
      Dim dblExtentWidth As Double = 0

      Double.TryParse(values(0).ToString(), dblHorizontalOffset)
      Double.TryParse(values(1).ToString(), dblViewportWidth)
      Double.TryParse(values(2).ToString(), dblExtentWidth)

      Dim fOnFarRight As Boolean = Math.Round((dblHorizontalOffset + dblViewportWidth), 2) < Math.Round(dblExtentWidth, 2)
      Return fOnFarRight
   End Function

   Public Function ConvertBack( _
               ByVal value As Object, _
               ByVal targetTypes As System.Type(), _
               ByVal parameter As Object, _
               ByVal culture As System.Globalization.CultureInfo _
            ) As Object() Implements IMultiValueConverter.ConvertBack
      Throw New System.NotImplementedException()
   End Function

End Class
