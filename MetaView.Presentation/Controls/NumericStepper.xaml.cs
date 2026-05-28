using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetaView.Presentation.Controls;

/// <summary>
/// Mouse-first numeric editor with step buttons and an optional slider popup.
/// </summary>
public partial class NumericStepper : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(NumericStepper),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumericStepper), new PropertyMetadata(0d, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumericStepper), new PropertyMetadata(double.MaxValue, OnRangeChanged));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double), typeof(NumericStepper), new PropertyMetadata(1d));

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(NumericStepper), new PropertyMetadata(0, OnFormatChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(NumericStepper), new PropertyMetadata(string.Empty, OnFormatChanged));

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(NumericStepper), new PropertyMetadata("0"));

    public static readonly DependencyProperty InputTextProperty =
        DependencyProperty.Register(nameof(InputText), typeof(string), typeof(NumericStepper), new PropertyMetadata("0"));

    public static readonly DependencyProperty IsPopupOpenProperty =
        DependencyProperty.Register(nameof(IsPopupOpen), typeof(bool), typeof(NumericStepper), new PropertyMetadata(false));

    public NumericStepper()
    {
        InitializeComponent();
        UpdateDisplayText();
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextProperty, value);
    }

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    public bool IsPopupOpen
    {
        get => (bool)GetValue(IsPopupOpenProperty);
        set => SetValue(IsPopupOpenProperty, value);
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var control = (NumericStepper)d;
        var value = (double)baseValue;
        var rounded = Math.Round(value, Math.Max(0, control.DecimalPlaces));

        if (double.IsNaN(control.Minimum) || double.IsNaN(control.Maximum) || control.Minimum > control.Maximum)
        {
            return rounded;
        }

        var clamped = Math.Clamp(rounded, control.Minimum, control.Maximum);
        return clamped;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NumericStepper)d).UpdateDisplayText();
    }

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericStepper)d;
        control.CoerceValue(ValueProperty);
        control.UpdateDisplayText();
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((NumericStepper)d).CoerceValue(ValueProperty);
    }

    private void OnIncreaseClick(object sender, RoutedEventArgs e)
    {
        ChangeValue(Step);
    }

    private void OnDecreaseClick(object sender, RoutedEventArgs e)
    {
        ChangeValue(-Step);
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsMouseOver)
        {
            return;
        }

        ChangeValue(e.Delta > 0 ? Step : -Step);
        e.Handled = true;
    }

    private void OnValueTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        IsPopupOpen = true;
        if (!ValueTextBox.IsMouseCaptureWithin)
        {
            ValueTextBox.SelectAll();
        }
    }

    private void OnValueTextBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        IsPopupOpen = true;
    }

    private void OnValueTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        CommitInputText();
    }

    private void OnValueTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitInputText();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            InputText = DisplayText;
            IsPopupOpen = false;
            Keyboard.ClearFocus();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            CommitInputText();
            ChangeValue(Step);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Down)
        {
            CommitInputText();
            ChangeValue(-Step);
            e.Handled = true;
        }
    }

    private void ChangeValue(double delta)
    {
        Value = Math.Round(Value + delta, Math.Max(0, DecimalPlaces));
    }

    private void CommitInputText()
    {
        var text = InputText?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(Unit) && text.EndsWith(Unit, StringComparison.OrdinalIgnoreCase))
        {
            text = text[..^Unit.Length].Trim();
        }

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var currentCultureValue) ||
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out currentCultureValue))
        {
            Value = currentCultureValue;
            UpdateDisplayText(forceInputText: true);
            return;
        }

        InputText = DisplayText;
    }

    private void UpdateDisplayText(bool forceInputText = false)
    {
        var places = Math.Max(0, DecimalPlaces);
        var format = places == 0 ? "0" : "0." + new string('0', places);
        var valueText = Value.ToString(format, CultureInfo.InvariantCulture);
        DisplayText = string.IsNullOrWhiteSpace(Unit) ? valueText : $"{valueText} {Unit}";
        if (forceInputText || !ValueTextBox.IsKeyboardFocusWithin)
        {
            InputText = DisplayText;
        }
    }
}

