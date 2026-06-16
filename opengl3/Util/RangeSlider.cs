using System;
using System.Drawing;
using System.Windows.Forms;


public class RangeSliderH : UserControl
{
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 100;

    private int _lowerValue = 20;
    public int LowerValue
    {
        get => _lowerValue;
        set { _lowerValue = Math.Max(MinValue, value); Invalidate(); }
    }

    private int _upperValue = 80;
    public int UpperValue
    {
        get => _upperValue;
        set { _upperValue = Math.Min(MaxValue, value); Invalidate(); }
    }

    private bool draggingLower = false;
    private bool draggingUpper = false;

    public event EventHandler ValuesChanged;

    public RangeSliderH()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.Clear(BackColor);

        // Размеры ползунков
        int thumbWidth = 12;
        int thumbHeight = Height;

        // Координаты для рисования линии
        int lineY = Height / 2 - 2;
        int lineWidth = Width - thumbWidth;

        // Позиции ползунков (в пикселях)
        int lowerX = (int)((double)(LowerValue - MinValue) / (MaxValue - MinValue) * lineWidth);
        int upperX = (int)((double)(UpperValue - MinValue) / (MaxValue - MinValue) * lineWidth) + thumbWidth / 2;

        // Рисуем фоновую линию
        g.FillRectangle(Brushes.LightGray, 0, lineY, Width, 4);

        // Рисуем активную линию между ползунками
        g.FillRectangle(Brushes.DodgerBlue, lowerX + thumbWidth / 2, lineY, upperX - lowerX, 4);

        // Рисуем ползунки
        g.FillRectangle(Brushes.Gray, lowerX, 0, thumbWidth, thumbHeight);
        g.FillRectangle(Brushes.Gray, upperX, 0, thumbWidth, thumbHeight);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        int thumbWidth = 12;
        int lineWidth = Width - thumbWidth;

        int lowerX = (int)((double)(LowerValue - MinValue) / (MaxValue - MinValue) * lineWidth);
        int upperX = (int)((double)(UpperValue - MinValue) / (MaxValue - MinValue) * lineWidth) + thumbWidth / 2;

        // Определяем, за какой ползунок потянул пользователь
        if (e.X >= lowerX && e.X <= lowerX + thumbWidth)
        {
            draggingLower = true;
        }
        else if (e.X >= upperX && e.X <= upperX + thumbWidth)
        {
            draggingUpper = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        int thumbWidth = 12;
        int lineWidth = Width - thumbWidth;

        if (draggingLower)
        {
            double percent = (double)e.X / lineWidth;
            int val = (int)(percent * (MaxValue - MinValue)) + MinValue;
            val = Math.Max(MinValue, Math.Min(val, UpperValue));
            if (val != LowerValue)
            {
                LowerValue = val;
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (draggingUpper)
        {
            double percent = (double)(e.X - thumbWidth / 2) / lineWidth;
            int val = (int)(percent * (MaxValue - MinValue)) + MinValue;
            val = Math.Max(LowerValue, Math.Min(val, MaxValue));
            if (val != UpperValue)
            {
                UpperValue = val;
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        draggingLower = false;
        draggingUpper = false;
    }
}


public class RangeSliderV : UserControl
{
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 100;

    private int _lowerValue = 20;
    public int LowerValue
    {
        get => _lowerValue;
        set { _lowerValue = Math.Max(MinValue, Math.Min(value, UpperValue)); Invalidate(); }
    }

    private int _upperValue = 80;
    public int UpperValue
    {
        get => _upperValue;
        set { _upperValue = Math.Min(MaxValue, Math.Max(value, LowerValue)); Invalidate(); }
    }

    private bool draggingLower = false;
    private bool draggingUpper = false;

    public event EventHandler ValuesChanged;

    public RangeSliderV()
    {
        DoubleBuffered = true;
        // Опционально: задать минимальный размер
        Size = new Size(30, 200);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.Clear(BackColor);

        int thumbHeight = 12;          // высота каждого ползунка
        int thumbWidth = Width;        // ползунок на всю ширину контрола

        int lineX = Width / 2 - 2;     // вертикальная линия по центру
        int lineHeight = Height - thumbHeight;

        // Вычисляем Y-координаты ползунков (верхний = LowerValue, нижний = UpperValue)
        int lowerY = (int)((double)(MaxValue - LowerValue) / (MaxValue - MinValue) * lineHeight);
        int upperY = (int)((double)(MaxValue - UpperValue) / (MaxValue - MinValue) * lineHeight) + thumbHeight / 2;

        // Фоновая линия
        g.FillRectangle(Brushes.LightGray, lineX, 0, 4, Height);

        // Активная область между ползунками
        int activeTop = Math.Min(lowerY + thumbHeight / 2, upperY);
        int activeBottom = Math.Max(lowerY + thumbHeight / 2, upperY + thumbHeight / 2);
        g.FillRectangle(Brushes.DodgerBlue, lineX, activeTop, 4, activeBottom - activeTop);

        // Ползунки
        g.FillRectangle(Brushes.Gray, 0, lowerY, thumbWidth, thumbHeight);
        g.FillRectangle(Brushes.Gray, 0, upperY, thumbWidth, thumbHeight);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        int thumbHeight = 12;

        int lineHeight = Height - thumbHeight;
        int lowerY = (int)((double)(MaxValue - LowerValue) / (MaxValue - MinValue) * lineHeight);
        int upperY = (int)((double)(MaxValue - UpperValue) / (MaxValue - MinValue) * lineHeight) + thumbHeight / 2;

        if (e.Y >= lowerY && e.Y <= lowerY + thumbHeight)
            draggingLower = true;
        else if (e.Y >= upperY && e.Y <= upperY + thumbHeight)
            draggingUpper = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int thumbHeight = 12;
        int lineHeight = Height - thumbHeight;

        if (draggingLower)
        {
            double percent = 1.0 - (double)(e.Y - thumbHeight / 2) / lineHeight;
            //percent = Math.Clamp(percent, 0.0, 1.0);
            int val = (int)(percent * (MaxValue - MinValue)) + MinValue;
            val = Math.Max(MinValue, Math.Min(val, UpperValue));
            if (val != LowerValue)
            {
                LowerValue = val;
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (draggingUpper)
        {
            double percent = 1.0 - (double)(e.Y - thumbHeight / 2) / lineHeight;
            //percent = Math. p(percent, 0.0, 1.0);
            int val = (int)(percent * (MaxValue - MinValue)) + MinValue;
            val = Math.Max(LowerValue, Math.Min(val, MaxValue));
            if (val != UpperValue)
            {
                UpperValue = val;
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        draggingLower = false;
        draggingUpper = false;
    }
}

