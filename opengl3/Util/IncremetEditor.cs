using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace opengl3
{
    public class IncrementEditorInt : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Показываем кнопку с многоточием (...) в ячейке
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService =
                (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null && value is int)
            {
                int currentValue = (int)value;

                // Логика инкремента (например, увеличиваем на 1)
                currentValue += 1;

                return currentValue;
            }

            return base.EditValue(context, provider, value);
        }
    }

    public class IncrementEditorDouble : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Показываем кнопку с многоточием (...) в ячейке
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService =
                (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null && value is double)
            {
                double currentValue = (double)value;

                // Логика инкремента (например, увеличиваем на 1)
                currentValue += 1;

                return currentValue;
            }

            return base.EditValue(context, provider, value);
        }
    }

    public class DoubleUpDownEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Позволяет открывать выпадающий список (стрелочку)
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            }

            if (editorService != null && value is double)
            {
                // Создаем и настраиваем NumericUpDown
                NumericUpDown numericUpDown = new NumericUpDown
                {
                    Minimum = -1000, // Задайте нужные ограничения
                    Maximum = 1000,
                    DecimalPlaces = 2, // Количество знаков после запятой
                    Increment = 1,   // Шаг инкремента/декремента
                    Value =(decimal) (double)value
                };

                // Событие для обновления значения при изменении
                numericUpDown.ValueChanged += (sender, e) =>
                {
                    value = (double)numericUpDown.Value;
                };

                // Отображаем элемент в PropertyGrid
                editorService.DropDownControl(numericUpDown);

                // Возвращаем новое значение после закрытия
                return (double)numericUpDown.Value;
            }

            return value;
        }
    }

    public class DoubleUpDownEditorAngle : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Позволяет открывать выпадающий список (стрелочку)
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            }

            if (editorService != null && value is double)
            {
                // Создаем и настраиваем NumericUpDown
                NumericUpDown numericUpDown = new NumericUpDown
                {
                    Minimum = -180,
                    Maximum = 180,
                    DecimalPlaces = 2,
                    Increment = 1,   
                    Value = (decimal)((double)value)
                };

                // Событие для обновления значения при изменении
                numericUpDown.ValueChanged += (sender, e) =>
                {
                    value = (double)numericUpDown.Value;
                };

                // Отображаем элемент в PropertyGrid
                editorService.DropDownControl(numericUpDown);

                // Возвращаем новое значение после закрытия
                return (double)numericUpDown.Value;
            }

            return value;
        }
    }



}
