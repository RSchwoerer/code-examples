Imports System
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media
Imports System.Windows.Media.Animation

''' <summary>
''' A scrollable TabPanel control.
''' </summary>
Public Class ScrollableTabPanel
   Inherits Panel
   Implements IScrollInfo
   Implements INotifyPropertyChanged

#Region "--- Members ---"

   'For a description of the members below, refer to the respective property's description.
   Private _svOwningScrollViewer As ScrollViewer
   Private _fCanScroll_H As Boolean = True
   Private _szControlExtent As New Size(0, 0)
   Private _szViewport As New Size(0, 0)
   Private _vOffset As Vector

   'The following GradientStopCollections are being used for assigning an OpacityMask
   'to child-controls that are only partially visible.
   'For the VB-version, these cannot be initialized here, hence the GradientStops have been moved to the C'tor.
   Private Shared _gscOpacityMaskStops_TransparentOnLeftAndRight As New GradientStopCollection()
   Private Shared _gscOpacityMaskStops_TransparentOnLeft As New GradientStopCollection()
   Private Shared _gscOpacityMaskStops_TransparentOnRight As New GradientStopCollection()

   ''' <summary>
   ''' This will apply the present scroll-position resp. -offset.
   ''' </summary>
   Private _ttScrollTransform As New TranslateTransform()

#End Region

#Region "--- C'tor ---"

   Public Sub New()
      _gscOpacityMaskStops_TransparentOnLeftAndRight.Add(New GradientStop(Colors.Transparent, 0.0))
      _gscOpacityMaskStops_TransparentOnLeftAndRight.Add(New GradientStop(Colors.Black, 0.2))
      _gscOpacityMaskStops_TransparentOnLeftAndRight.Add(New GradientStop(Colors.Black, 0.8))
      _gscOpacityMaskStops_TransparentOnLeftAndRight.Add(New GradientStop(Colors.Transparent, 1.0))

      _gscOpacityMaskStops_TransparentOnLeft.Add(New GradientStop(Colors.Transparent, 0))
      _gscOpacityMaskStops_TransparentOnLeft.Add(New GradientStop(Colors.Black, 0.5))

      _gscOpacityMaskStops_TransparentOnRight.Add(New GradientStop(Colors.Black, 0.5))
      _gscOpacityMaskStops_TransparentOnRight.Add(New GradientStop(Colors.Transparent, 1))

      Me.RenderTransform = _ttScrollTransform
      AddHandler Me.SizeChanged, AddressOf ScrollableTabPanel_SizeChanged
   End Sub

#End Region

#Region "--- Helpers ---"

   ''' <summary>
   ''' Calculates the HorizontalOffset for a given child-control, based on a desired value.
   ''' </summary>
   ''' <param name="dblViewport_Left">The left offset of the Viewport.</param>
   ''' <param name="dblViewport_Right">The right offset of the Viewport.</param>
   ''' <param name="dblChild_Left">The left offset of the control in question.</param>
   ''' <param name="dblChild_Right">The right offset of the control in question.</param>
   ''' <returns></returns>
   Private Shared Function CalculateNewScrollOffset( _
               ByVal dblViewport_Left As Double, _
               ByVal dblViewport_Right As Double, _
               ByVal dblChild_Left As Double, _
               ByVal dblChild_Right As Double _
            ) As Double
      'Retrieve basic information about the position of the Viewport within the Extent of the control.
      Dim fIsFurtherToLeft As Boolean = (dblChild_Left < dblViewport_Left) AndAlso (dblChild_Right < dblViewport_Right)
      Dim fIsFurtherToRight As Boolean = (dblChild_Right > dblViewport_Right) AndAlso (dblChild_Left > dblViewport_Left)
      Dim fIsWiderThanViewport As Boolean = (dblChild_Right - dblChild_Left) > (dblViewport_Right - dblViewport_Left)

      If Not fIsFurtherToRight AndAlso Not fIsFurtherToLeft Then
         'Don't change anything - the Viewport is completely visible (inside the Extent's bounds)
         Return dblViewport_Left
      End If

      If fIsFurtherToLeft AndAlso Not fIsWiderThanViewport Then
         'The child is to be placed with its left edge equal to the left edge of the Viewport's present offset.
         Return dblChild_Left
      End If

      'The child is to be placed with its right edge equal to the right edge of the Viewport's present offset.
      Return (dblChild_Right - (dblViewport_Right - dblViewport_Left))
   End Function

   ''' <summary>
   ''' Compares the present sizes (Extent/Viewport) against the local values
   ''' and updates them, if required.
   ''' </summary>
   Private Sub UpdateMembers(ByVal szExtent As Size, ByVal szViewportSize As Size)
      If szExtent <> Me.Extent Then
         'The Extent of the control has changed.
         Me.Extent = szExtent
         If Me.ScrollOwner IsNot Nothing Then
            Me.ScrollOwner.InvalidateScrollInfo()
         End If
      End If

      If szViewportSize <> Me.Viewport Then
         'The Viewport of the panel has changed.
         Me.Viewport = szViewportSize
         If Me.ScrollOwner IsNot Nothing Then
            Me.ScrollOwner.InvalidateScrollInfo()
         End If
      End If

      'Prevent from getting off to the right
      If Me.HorizontalOffset + Me.Viewport.Width + Me.RightOverflowMargin > Me.ExtentWidth Then
         SetHorizontalOffset(HorizontalOffset + Me.Viewport.Width + Me.RightOverflowMargin)
      End If

      'Notify UI-subscribers
      NotifyPropertyChanged("CanScroll")
      NotifyPropertyChanged("CanScrollLeft")
      NotifyPropertyChanged("CanScrollRight")
   End Sub

   ''' <summary>
   ''' Returns the left position of the requested child (in Viewport-coordinates).
   ''' </summary>
   ''' <param name="uieChild">The child to retrieve the position for.</param>
   Private Function getLeftEdge(ByVal uieChild As UIElement) As Double
      Dim dblWidth As Double = 0
      Dim dblWidth_Total As Double = 0

      'Loop through all child controls, summing up their required width
      For Each uie As UIElement In Me.InternalChildren
         'The width of the current child control
         dblWidth = uie.DesiredSize.Width

         If uieChild IsNot Nothing AndAlso uieChild Is uie Then
            'The current child control is the one in question, so disregard its width
            'and return the total width required for all controls further to the left,
            'equaling the left edge of the requested child control.
            Return dblWidth_Total
         End If

         'Sum up the overall width while the child control in question hasn't been hit.
         dblWidth_Total += dblWidth
      Next

      'This shouldn't really be hit as the requested control should've been found beforehand.
      Return dblWidth_Total
   End Function

   ''' <summary>
   ''' Determines whether the passed child control is only partially visible
   ''' (i.e. whether part of it is outside of the Viewport).
   ''' </summary>
   ''' <param name="uieChild">The child control to be tested.</param>
   ''' <returns>
   ''' True if part of the control is further to the left or right of the
   ''' Viewport, False otherwise.
   ''' </returns>
   Public Function IsPartlyVisible(ByVal uieChild As UIElement) As Boolean
      Dim rctIntersect As Rect = GetIntersectionRectangle(uieChild)
      Return (Not (rctIntersect = Rect.Empty))
   End Function

   ''' <summary>
   ''' Determines the visible part of the passed child control, 
   ''' measured between 0 (completely invisible) and 1 (completely visible),
   ''' that is overflowing into the right invisible portion of the panel.
   ''' </summary>
   ''' <param name="uieChild">The child control to be tested.</param>
   ''' <returns>
   ''' <para>A number between 0 (the control is completely invisible resp. outside of
   ''' the Viewport) and 1 (the control is completely visible).</para>
   ''' <para>All values between 0 and 1 indicate the part that is visible
   ''' (i.e. 0.4 would mean that 40% of the control is visible, the remaining
   ''' 60% will overflow into the right invisible portion of the panel.</para>
   ''' </returns>
   Public Function PartlyVisiblePortion_OverflowToRight(ByVal uieChild As UIElement) As Double
      Dim rctIntersect As Rect = GetIntersectionRectangle(uieChild)
      Dim dblVisiblePortion As Double = 1
      If Not (rctIntersect = Rect.Empty) AndAlso Me.CanScrollRight AndAlso rctIntersect.Width < uieChild.DesiredSize.Width AndAlso rctIntersect.X > 0 Then
         dblVisiblePortion = rctIntersect.Width / uieChild.DesiredSize.Width
      End If

      Return dblVisiblePortion
   End Function

   ''' <summary>
   ''' Determines the visible part of the passed child control, 
   ''' measured between 0 (completely invisible) and 1 (completely visible),
   ''' that is overflowing into the left invisible portion of the panel.
   ''' </summary>
   ''' <param name="uieChild">The child control to be tested.</param>
   ''' <returns>
   ''' <para>A number between 0 (the control is completely invisible resp. outside of
   ''' the Viewport) and 1 (the control is completely visible).</para>
   ''' <para>All values between 0 and 1 indicate the part that is visible
   ''' (i.e. 0.4 would mean that 40% of the control is visible, the remaining
   ''' 60% will overflow into the left invisible portion of the panel.</para>
   ''' </returns>
   Public Function PartlyVisiblePortion_OverflowToLeft(ByVal uieChild As UIElement) As Double
      Dim rctIntersect As Rect = GetIntersectionRectangle(uieChild)
      Dim dblVisiblePortion As Double = 1
      If Not (rctIntersect = Rect.Empty) AndAlso Me.CanScrollLeft AndAlso rctIntersect.Width < uieChild.DesiredSize.Width AndAlso rctIntersect.X = 0 Then
         dblVisiblePortion = rctIntersect.Width / uieChild.DesiredSize.Width
      End If

      Return dblVisiblePortion
   End Function

   ''' <summary>
   ''' Returns the currently rendered rectangle that makes up the Viewport.
   ''' </summary>
   Private Function GetScrollViewerRectangle() As Rect
      Return New Rect(New Point(0, 0), Me.ScrollOwner.RenderSize)
   End Function

   ''' <summary>
   ''' Returns the rectangle that defines the outer bounds of a child control.
   ''' </summary>
   ''' <param name="uieChild">The child/control for which to return the bounding rectangle.</param>
   Private Function GetChildRectangle(ByVal uieChild As UIElement) As Rect
      'Retrieve the position of the requested child inside the ScrollViewer control
      Dim childTransform As GeneralTransform = uieChild.TransformToAncestor(Me.ScrollOwner)
      Return childTransform.TransformBounds(New Rect(New Point(0, 0), uieChild.RenderSize))
   End Function

   ''' <summary>
   ''' Returns a Rectangle that contains the intersection between the ScrollViewer's
   ''' and the passed child control's boundaries, that is, the portion of the child control
   ''' which is currently visibile within the ScrollViewer's Viewport.
   ''' </summary>
   ''' <param name="uieChild">The child for which to retrieve Rectangle.</param>
   ''' <returns></returns>
   Private Function GetIntersectionRectangle(ByVal uieChild As UIElement) As Rect
      'Retrieve the ScrollViewer's rectangle
      Dim rctScrollViewerRectangle As Rect = GetScrollViewerRectangle()
      Dim rctChildRect As Rect = GetChildRectangle(uieChild)

      'Return the area/rectangle in which the requested child and the ScrollViewer control's Viewport intersect.
      Return Rect.Intersect(rctScrollViewerRectangle, rctChildRect)
   End Function

   ''' <summary>
   ''' Will remove the OpacityMask for all child controls.
   ''' </summary>
   Private Sub RemoveOpacityMasks()
      For Each uieChild As UIElement In Children
         RemoveOpacityMask(uieChild)
      Next
   End Sub

   ''' <summary>
   ''' Will remove the OpacityMask for all child controls.
   ''' </summary>
   Private Sub RemoveOpacityMask(ByVal uieChild As UIElement)
      uieChild.OpacityMask = Nothing
   End Sub

   ''' <summary>
   ''' Will check all child controls and set their OpacityMasks.
   ''' </summary>
   Private Sub UpdateOpacityMasks()
      For Each uieChild As UIElement In Children
         UpdateOpacityMask(uieChild)
      Next
   End Sub

   ''' <summary>
   ''' Takes the given child control and checks as to whether the control is completely
   ''' visible (in the Viewport). If not (i.e. if it's only partially visible), an OpacityMask
   ''' will be applied so that it fades out into nothingness.
   ''' </summary>
   Private Sub UpdateOpacityMask(ByVal uieChild As UIElement)
      If uieChild Is Nothing Then Return

      'Retrieve the ScrollViewer's rectangle
      Dim rctScrollViewerRectangle As Rect = GetScrollViewerRectangle()
      If rctScrollViewerRectangle = Rect.Empty Then Return

      'Retrieve the child control's rectangle
      Dim rctChildRect As Rect = GetChildRectangle(uieChild)

      If rctScrollViewerRectangle.Contains(rctChildRect) Then
         'This child is completely visible, so dump the OpacityMask.
         uieChild.OpacityMask = Nothing
      Else
         Dim dblPartlyVisiblePortion_OverflowToLeft As Double = PartlyVisiblePortion_OverflowToLeft(uieChild)
         Dim dblPartlyVisiblePortion_OverflowToRight As Double = PartlyVisiblePortion_OverflowToRight(uieChild)

         If dblPartlyVisiblePortion_OverflowToLeft < 1 AndAlso dblPartlyVisiblePortion_OverflowToRight < 1 Then
            uieChild.OpacityMask = New LinearGradientBrush(_gscOpacityMaskStops_TransparentOnLeftAndRight, New Point(0, 0), New Point(1, 0))
         ElseIf dblPartlyVisiblePortion_OverflowToLeft < 1 Then
            'A part of the child (to the left) remains invisible, so fade out to the left.
            uieChild.OpacityMask = New LinearGradientBrush( _
                  _gscOpacityMaskStops_TransparentOnLeft, _
                  New Point(1 - dblPartlyVisiblePortion_OverflowToLeft, 0), _
                  New Point(1, 0) _
               )
         ElseIf dblPartlyVisiblePortion_OverflowToRight < 1 Then
            'A part of the child (to the right) remains invisible, so fade out to the right.
            uieChild.OpacityMask = New LinearGradientBrush( _
                  _gscOpacityMaskStops_TransparentOnRight, _
                  New Point(0, 0), _
                  New Point(dblPartlyVisiblePortion_OverflowToRight, 0) _
               )
         Else
            'This child is completely visible, so dump the OpacityMask.
            'Actually, this part should never be reached as, in this case, the very first
            'checkup should've resulted in the child-rect being completely contained in
            'the SV's rect; Well, I'll leave this here anyhow (just to be save).
            uieChild.OpacityMask = Nothing
         End If
      End If
   End Sub

#End Region

#Region "--- Overrides ---"

   ''' <summary>
   ''' This is the 1st pass of the layout process. Here, the Extent's size is being determined.
   ''' </summary>
   ''' <param name="availableSize">The Viewport's rectangle, as obtained after the 1st pass (MeasureOverride).</param>
   ''' <returns>The Viewport's final size.</returns>
   Protected Overloads Overrides Function MeasureOverride(ByVal availableSize As Size) As Size
      'The default size will not reflect any width (i.e., no children) and always the default height.
      Dim resultSize As New Size(0, availableSize.Height)

      'Loop through all child controls ...
      For Each uieChild As UIElement In Me.InternalChildren
         '... retrieve the desired size of the control ...
         uieChild.Measure(availableSize)
         '... and pass this on to the size we need for the Extent
         resultSize.Width += uieChild.DesiredSize.Width
      Next

      UpdateMembers(resultSize, availableSize)

      Dim dblNewWidth As Double = If(Double.IsPositiveInfinity(availableSize.Width), resultSize.Width, availableSize.Width)

      resultSize.Width = dblNewWidth
      Return resultSize
   End Function

   ''' <summary>
   ''' This is the 2nd pass of the layout process, where child controls are
   ''' being arranged within the panel.
   ''' </summary>
   ''' <param name="finalSize">The Viewport's rectangle, as obtained after the 1st pass (MeasureOverride).</param>
   ''' <returns>The Viewport's final size.</returns>
   Protected Overloads Overrides Function ArrangeOverride(ByVal finalSize As Size) As Size
      If Me.InternalChildren Is Nothing OrElse Me.InternalChildren.Count < 1 Then
         Return finalSize
      End If

      Dim dblWidth As Double = 0
      Dim dblWidth_Total As Double = 0
      For Each uieChild As UIElement In Me.InternalChildren
         dblWidth = uieChild.DesiredSize.Width
         uieChild.Arrange(New Rect(dblWidth_Total, 0, dblWidth, uieChild.DesiredSize.Height))
         dblWidth_Total += dblWidth
      Next

      Return finalSize
   End Function

   Protected Overloads Overrides Sub OnVisualChildrenChanged(ByVal visualAdded As DependencyObject, ByVal visualRemoved As DependencyObject)
      MyBase.OnVisualChildrenChanged(visualAdded, visualRemoved)
      UpdateOpacityMasks()
   End Sub

   Protected Overloads Overrides Sub OnChildDesiredSizeChanged(ByVal child As UIElement)
      MyBase.OnChildDesiredSizeChanged(child)
      UpdateOpacityMasks()
   End Sub

#End Region

#Region "--- IScrollInfo Members ---"

   ''' <summary>
   ''' Sets or retrieves whether the control is allowed to scroll horizontally.
   ''' </summary>
   Public Property CanHorizontallyScroll() As Boolean Implements IScrollInfo.CanHorizontallyScroll
      Get
         Return _fCanScroll_H
      End Get
      Set(ByVal value As Boolean)
         _fCanScroll_H = value
      End Set
   End Property

   ''' <summary>
   ''' Sets or retrieves whether the control is allowed to scroll vertically.
   ''' </summary>
   ''' <remarks>
   ''' This is DISABLED for the control! Due to the internal plumbing of the ScrollViewer
   ''' control, this property needs to be accessible without an exception being thrown;
   ''' however, setting this property will do plain nothing.
   ''' </remarks>
   Public Property CanVerticallyScroll() As Boolean Implements IScrollInfo.CanVerticallyScroll
      'We'll never be able to vertically scroll.
      Get
         Return False
      End Get
      Set(ByVal value As Boolean)
      End Set
   End Property

   ''' <summary>
   ''' Retrieves the height of the control; since no vertical scrolling has been
   ''' implemented, this will return the same value at all times.
   ''' </summary>
   Public ReadOnly Property ExtentHeight() As Double Implements IScrollInfo.ExtentHeight
      Get
         Return Me.Extent.Height
      End Get
   End Property

   ''' <summary>
   ''' Retrieves the overall width of the content hosted in the panel (i.e., the width
   ''' measured between [far left of the scrollable portion] and [far right of the scrollable portion].
   ''' </summary>
   Public ReadOnly Property ExtentWidth() As Double Implements IScrollInfo.ExtentWidth
      Get
         Return Me.Extent.Width
      End Get
   End Property

   ''' <summary>
   ''' Retrieves the current horizontal scroll offset.
   ''' </summary>
   ''' <remarks>The setter is private to the class.</remarks>
   Public ReadOnly Property HorizontalOffset() As Double Implements IScrollInfo.HorizontalOffset
      Get
         Return _vOffset.X
      End Get
      'We cannot have a private setter in VB as the interface requires a ReadOnly property.
      'I have thus replaced the C#-calls to the setter with direct references to _vOffset.X here.
      'Private Set(ByVal value As Double)
      '   _vOffset.X = value
      'End Set
   End Property

   ''' <summary>
   ''' Increments the vertical offset.
   ''' </summary>
   ''' <remarks>This is unsupported.</remarks>
   Public Sub LineDown() Implements IScrollInfo.LineDown
      Throw New InvalidOperationException()
   End Sub

   ''' <summary>
   ''' Decrements the horizontal offset by the amount specified in the <see cref="LineScrollPixelCount"/> property.
   ''' </summary>
   Public Sub LineLeft() Implements IScrollInfo.LineLeft
      SetHorizontalOffset(Me.HorizontalOffset - Me.LineScrollPixelCount)
   End Sub

   ''' <summary>
   ''' Increments the horizontal offset by the amount specified in the <see cref="LineScrollPixelCount"/> property.
   ''' </summary>
   Public Sub LineRight() Implements IScrollInfo.LineRight
      SetHorizontalOffset(Me.HorizontalOffset + Me.LineScrollPixelCount)
   End Sub

   ''' <summary>
   ''' Decrements the vertical offset.
   ''' </summary>
   ''' <remarks>This is unsupported.</remarks>
   Public Sub LineUp() Implements IScrollInfo.LineUp
      Throw New InvalidOperationException()
   End Sub

   ''' <summary>
   ''' Scrolls a child of the panel (Visual) into view.
   ''' </summary>
   Public Function MakeVisible( _
               ByVal visual As System.Windows.Media.Visual, _
               ByVal rectangle As System.Windows.Rect _
            ) As System.Windows.Rect Implements IScrollInfo.MakeVisible
      If rectangle.IsEmpty OrElse visual Is Nothing OrElse visual Is Me OrElse Not MyBase.IsAncestorOf(visual) Then Return Rect.Empty

      Dim dblOffsetX As Double = 0
      Dim uieControlToMakeVisible As UIElement = Nothing
      For i As Integer = 0 To Me.InternalChildren.Count - 1
         If DirectCast(Me.InternalChildren(i), Visual) Is visual Then
            uieControlToMakeVisible = Me.InternalChildren(i)
            dblOffsetX = getLeftEdge(Me.InternalChildren(i))
            Exit For
         End If
      Next i

      'Set the offset only if the desired element is not already completely visible.
      If uieControlToMakeVisible IsNot Nothing Then
         If uieControlToMakeVisible Is Me.InternalChildren(0) Then
            'If the first child has been selected, go to the very beginning of the scrollable area
            dblOffsetX = 0
         ElseIf uieControlToMakeVisible Is Me.InternalChildren(Me.InternalChildren.Count - 1) Then
            'If the last child has been selected, go to the very end of the scrollable area
            dblOffsetX = Me.ExtentWidth - Me.Viewport.Width
         Else
            dblOffsetX = CalculateNewScrollOffset( _
                     Me.HorizontalOffset, _
                     Me.HorizontalOffset + Me.Viewport.Width, _
                     dblOffsetX, _
                     dblOffsetX + uieControlToMakeVisible.DesiredSize.Width _
                  )
         End If

         SetHorizontalOffset(dblOffsetX)
         rectangle = New Rect(Me.HorizontalOffset, 0, uieControlToMakeVisible.DesiredSize.Width, Me.Viewport.Height)
      End If

      Return rectangle
   End Function

   Public Sub MouseWheelDown() Implements IScrollInfo.MouseWheelDown
      'We won't be responding to the mouse-wheel.
   End Sub

   Public Sub MouseWheelLeft() Implements IScrollInfo.MouseWheelLeft
      'We won't be responding to the mouse-wheel.
   End Sub

   Public Sub MouseWheelRight() Implements IScrollInfo.MouseWheelRight
      'We won't be responding to the mouse-wheel.
   End Sub

   Public Sub MouseWheelUp() Implements IScrollInfo.MouseWheelUp
      'We won't be responding to the mouse-wheel.
   End Sub

   Public Sub PageDown() Implements IScrollInfo.PageDown
      'We won't be responding to vertical paging.
   End Sub

   Public Sub PageLeft() Implements IScrollInfo.PageLeft
      'We won't be responding to horizontal paging.
   End Sub

   Public Sub PageRight() Implements IScrollInfo.PageRight
      'We won't be responding to horizontal paging.
   End Sub

   Public Sub PageUp() Implements IScrollInfo.PageUp
      'We won't be responding to vertical paging.
   End Sub

   ''' <summary>
   ''' Sets or retrieves the ScrollViewer control that hosts the panel.
   ''' </summary>
   Public Property ScrollOwner() As ScrollViewer Implements IScrollInfo.ScrollOwner
      Get
         Return _svOwningScrollViewer
      End Get
      Set(ByVal value As ScrollViewer)
         _svOwningScrollViewer = value
         If _svOwningScrollViewer IsNot Nothing Then
            AddHandler Me.ScrollOwner.Loaded, AddressOf ScrollOwner_Loaded
         Else
            RemoveHandler Me.ScrollOwner.Loaded, AddressOf ScrollOwner_Loaded
         End If
      End Set
   End Property

   Public Sub SetHorizontalOffset(ByVal offset As Double) Implements IScrollInfo.SetHorizontalOffset
      'Remove all OpacityMasks while scrolling.
      RemoveOpacityMasks()

      'Assure that the horizontal offset always contains a valid value
      _vOffset.X = Math.Max(0, Math.Min(Me.ExtentWidth - Me.Viewport.Width, Math.Max(0, offset)))

      If Me.ScrollOwner IsNot Nothing Then
         Me.ScrollOwner.InvalidateScrollInfo()
      End If

      'If you don't want the animation, you would replace all the code further below (up to but not including)
      'the call to InvalidateMeasure() with the following line:
      '_ttScrollTransform.X = (this.HorizontalOffset * -1)

      'Animate the new offset
      Dim daScrollAnimation As New DoubleAnimation(_ttScrollTransform.X, (-Me.HorizontalOffset), New Duration(Me.AnimationTimeSpan), FillBehavior.HoldEnd)

      'Note that, depending on distance between the original and the target scroll-position and
      'the duration of the animation, the  acceleration and deceleration effects might be more
      'or less unnoticeable at runtime.
      daScrollAnimation.AccelerationRatio = 0.5
      daScrollAnimation.DecelerationRatio = 0.5

      'The childrens' OpacityMask can only be set reliably after the scroll-animation
      'has finished its work, so attach to the animation's Completed event where the
      'masks will be re-created.
      AddHandler daScrollAnimation.Completed, AddressOf daScrollAnimation_Completed

      _ttScrollTransform.BeginAnimation(TranslateTransform.XProperty, daScrollAnimation, HandoffBehavior.Compose)

      InvalidateMeasure()
   End Sub

   Public Sub SetVerticalOffset(ByVal offset As Double) Implements IScrollInfo.SetVerticalOffset
      Throw New InvalidOperationException()
   End Sub

   Public ReadOnly Property VerticalOffset() As Double Implements IScrollInfo.VerticalOffset
      Get
         Return 0
      End Get
   End Property

   Public ReadOnly Property ViewportHeight() As Double Implements IScrollInfo.ViewportHeight
      Get
         Return Me.Viewport.Height
      End Get
   End Property

   Public ReadOnly Property ViewportWidth() As Double Implements IScrollInfo.ViewportWidth
      Get
         Return Me.Viewport.Width
      End Get
   End Property

#End Region

#Region "--- Additional Properties ---"

   ''' <summary>
   ''' Retrieves the overall resp. internal/inner size of the control/panel.
   ''' </summary>
   ''' <remarks>The setter is private to the class.</remarks>
   Public Property Extent() As Size
      Get
         Return _szControlExtent
      End Get
      Private Set(ByVal value As Size)
         _szControlExtent = value
      End Set
   End Property

   ''' <summary>
   ''' Retrieves the outer resp. visible size of the control/panel.
   ''' </summary>
   ''' <remarks>The setter is private to the class.</remarks>
   Public Property Viewport() As Size
      Get
         Return _szViewport
      End Get
      Private Set(ByVal value As Size)
         _szViewport = value
      End Set
   End Property


   ''' <summary>
   ''' Retrieves whether the panel's scroll-position is on the far left (i.e. cannot scroll further to the left).
   ''' </summary>
   Public ReadOnly Property IsOnFarLeft() As Boolean
      Get
         Return Me.HorizontalOffset = 0
      End Get
   End Property

   ''' <summary>
   ''' Retrieves whether the panel's scroll-position is on the far right (i.e. cannot scroll further to the right).
   ''' </summary>
   Public ReadOnly Property IsOnFarRight() As Boolean
      Get
         Return (Me.HorizontalOffset + Me.Viewport.Width) = Me.ExtentWidth
      End Get
   End Property

   ''' <summary>
   ''' Retrieves whether the panel's viewport is larger than the control's extent, meaning there is hidden content 
   ''' that the user would have to scroll for in order to see it.
   ''' </summary>
   Public ReadOnly Property CanScroll() As Boolean
      Get
         Return Me.ExtentWidth > Me.Viewport.Width
      End Get
   End Property

   ''' <summary>
   ''' Retrieves whether the panel's scroll-position is NOT on the far left (i.e. can scroll to the left).
   ''' </summary>
   Public ReadOnly Property CanScrollLeft() As Boolean
      Get
         Return Me.CanScroll AndAlso Not Me.IsOnFarLeft
      End Get
   End Property

   ''' <summary>
   ''' Retrieves whether the panel's scroll-position is NOT on the far right (i.e. can scroll to the right).
   ''' </summary>
   Public ReadOnly Property CanScrollRight() As Boolean
      Get
         Return Me.CanScroll AndAlso Not Me.IsOnFarRight
      End Get
   End Property

#End Region

#Region "--- Additional Dependency Properties ---"

   Public Shared RightOverflowMarginProperty As DependencyProperty = _
      DependencyProperty.Register( _
            "RightOverflowMargin", _
            GetType(Integer), _
            GetType(ScrollableTabPanel), _
            New FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender) _
         )

   ''' <summary>
   ''' Sets or retrieves the Margin that will be applied to the rightmost item in the panel;
   ''' This allows for the item applying a negative margin, i.e. when selected.
   ''' If set to a value other than zero (being the default), the control will add the value
   ''' specified here to the item's right extent.
   ''' </summary>
   Public Property RightOverflowMargin() As Integer
      Get
         Return DirectCast(GetValue(RightOverflowMarginProperty), Integer)
      End Get
      Set(ByVal value As Integer)
         SetValue(RightOverflowMarginProperty, value)
      End Set
   End Property

   Public Shared AnimationTimeSpanProperty As DependencyProperty = _
      DependencyProperty.Register( _
            "AnimationTimeSpanProperty", _
            GetType(TimeSpan), _
            GetType(ScrollableTabPanel), _
            New FrameworkPropertyMetadata(New TimeSpan(0, 0, 0, 0, 100), FrameworkPropertyMetadataOptions.AffectsRender) _
         )
   ''' <summary>
   ''' Sets or retrieves the the duration (default: 100ms) for the panel's transition-animation that is
   ''' started when an item is selected (scroll from the previously selected item to the
   ''' presently selected one).
   ''' </summary>
   Public Property AnimationTimeSpan() As TimeSpan
      Get
         Return DirectCast(GetValue(AnimationTimeSpanProperty), TimeSpan)
      End Get
      Set(ByVal value As TimeSpan)
         SetValue(AnimationTimeSpanProperty, value)
      End Set
   End Property

   'The amount of pixels to scroll by for the LineLeft() and LineRight() methods.
   Public Shared LineScrollPixelCountProperty As DependencyProperty = _
      DependencyProperty.Register( _
            "LineScrollPixelCount", _
            GetType(Integer), _
            GetType(ScrollableTabPanel), _
            New FrameworkPropertyMetadata(15, FrameworkPropertyMetadataOptions.AffectsRender) _
         )
   ''' <summary>
   ''' Sets or retrieves the count of pixels to scroll by when the LineLeft or LineRight methods
   ''' are called (default: 15px).
   ''' </summary>
   Public Property LineScrollPixelCount() As Integer
      Get
         Return DirectCast(GetValue(LineScrollPixelCountProperty), Integer)
      End Get
      Set(ByVal value As Integer)
         SetValue(LineScrollPixelCountProperty, value)
      End Set
   End Property

#End Region

#Region "--- INotifyPropertyChanged ---"

   'Public Event PropertyChanged( _
   '         ByVal sender As Object, _
   '         ByVal e As System.ComponentModel.PropertyChangedEventArgs _
   '      ) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

   'Private Sub NotifyPropertyChanged(ByVal strPropertyName As String) Implements INotifyPropertyChanged
   '   RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(strPropertyName))
   'End Sub

   ''' <summary>
   ''' Raised when a property on this object has a new value.
   ''' </summary>
   Public Event PropertyChanged As PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

   ''' <summary>
   ''' Called from within this class whenever subscribers (i.e. bindings) are to be notified of a property-change
   ''' </summary>
   ''' <param name="strPropertyName">The name of the property that has changed.</param>
   Protected Sub NotifyPropertyChanged(ByVal strPropertyName As String)
      Dim handler As PropertyChangedEventHandler = Me.PropertyChangedEvent

      If handler IsNot Nothing Then
         handler.Invoke(Me, New PropertyChangedEventArgs(strPropertyName))
      End If
   End Sub

#End Region

#Region "--- Event Handlers ---"

   ''' <summary>
   ''' Fired when the ScrollViewer is initially loaded/displayed. 
   ''' Required in order to initially setup the childrens' OpacityMasks.
   ''' </summary>
   Sub ScrollOwner_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
      UpdateOpacityMasks()
   End Sub

   ''' <summary>
   ''' Fired when the scroll-animation has finished its work, that is, at the
   ''' point in time when the ScrollViewerer has reached its final scroll-position
   ''' resp. offset, which is when the childrens' OpacityMasks can be updated.
   ''' </summary>
   Sub daScrollAnimation_Completed(ByVal sender As Object, ByVal e As EventArgs)
      UpdateOpacityMasks()

      'This is required in order to update the TabItems' FocusVisual
      For Each uieChild As UIElement In Me.InternalChildren
         uieChild.InvalidateArrange()
      Next
   End Sub

   Sub ScrollableTabPanel_SizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs)
      UpdateOpacityMasks()
   End Sub

#End Region

End Class


