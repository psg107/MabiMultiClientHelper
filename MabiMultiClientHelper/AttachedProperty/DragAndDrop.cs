using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MabiMultiClientHelper.AttachedProperty
{
    /// <summary>
    /// https://six605.tistory.com/428
    /// https://www.codeproject.com/Articles/22855/ListBox-Drag-Drop-using-Attached-Properties
    /// </summary>
    public class DragAndDrop
    {
        #region DragEnabled 

        public static readonly DependencyProperty DragEnabledProperty =
        DependencyProperty.RegisterAttached("DragEnabled", typeof(bool),
            typeof(DragAndDrop), new FrameworkPropertyMetadata(OnDragEnabledChanged));

        public static void SetDragEnabled(DependencyObject element, bool value)
        {
            element.SetValue(DragEnabledProperty, value);
        }

        public static bool GetDragEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(DragEnabledProperty);
        }

        public static void OnDragEnabledChanged
            (DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue == true)
            {
                ListBox listbox = (ListBox)obj;
                listbox.PreviewMouseLeftButtonDown +=
                    new MouseButtonEventHandler(Listbox_PreviewMouseLeftButtonDown);
            }
        }

        #endregion

        #region DropEnabled 

        public static readonly DependencyProperty DropEnabledProperty =
        DependencyProperty.RegisterAttached("DropEnabled", typeof(bool),
            typeof(DragAndDrop), new FrameworkPropertyMetadata(OnDropEnabledChanged));

        public static void SetDropEnabled(DependencyObject element, bool value)
        {
            element.SetValue(DropEnabledProperty, value);
        }

        public static bool GetDropEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(DropEnabledProperty);
        }

        public static void OnDropEnabledChanged
            (DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue == true)
            {
                ListBox listbox = (ListBox)obj;
                listbox.AllowDrop = true;
                listbox.Drop += new DragEventHandler(Listbox_Drop);
            }
        }

        private static ListBox dragSource;
        private static Type dragType;

        static void Listbox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            dragSource = parent;

            object data = GetObjectDataFromPoint(dragSource, e.GetPosition(parent));

            //주석 풀면 포커스가 된 상태에서만 드래그 가능해짐.. 넣었다가 불편해서 제거
            //if (dragSource.SelectedItem != data)
            //{
            //    return;
            //}

            if (data != null)
            {
                dragType = data.GetType();
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }

        static void Listbox_Drop(object sender, DragEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            object data = e.Data.GetData(dragType);
            if (dragType.IsVisible == true)
            {
                ((IList)dragSource.ItemsSource).Remove(data);
                ((IList)parent.ItemsSource).Add(data);
            }
        }

        #endregion

        #region Helper 

        private static object GetObjectDataFromPoint(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    if (element == source)
                        return null;
                }
                if (data != DependencyProperty.UnsetValue)
                    return data;
            }
            return null;
        }

        #endregion
    }
}
