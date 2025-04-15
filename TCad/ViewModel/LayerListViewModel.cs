using Plotter;
using Plotter.Controller;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TCad.ViewModel;

public class LayerListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(String info)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    public ObservableCollection<LayerHolder> LayerList_ = new ObservableCollection<LayerHolder>();
    public ObservableCollection<LayerHolder> LayerList
    {
        get
        {
            return LayerList_;
        }
        set
        {
            LayerList_ = value;
            NotifyPropertyChanged("LayerList");
        }
    }

    public LayerHolder SelectedItem
    {
        get
        {
            int idx = GetLayerListIndex(Controller.CurrentLayer.ID);
            if (idx < 0)
            {
                return null;
            }
            return LayerList[idx];
        }

        set
        {
            LayerHolder layerHolder = value;
            if (layerHolder == null)
            {
                return;
            }

            if (Controller.CurrentLayer.ID != layerHolder.ID)
            {
                Controller.SetCurrentLayer(layerHolder.ID);

                Controller.Drawer.Redraw();
            }

            NotifyPropertyChanged("SelectedItem");
        }
    }

    protected IPlotterController Controller;

    public LayerListViewModel(IPlotterController controller)
    {
        Controller = controller;
    }

    public void LayerListItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        LayerHolder lh = (LayerHolder)sender;
        Controller.Drawer.Redraw();
    }

    public void LayerListChanged(LayerListInfo layerListInfo)
    {
        foreach (LayerHolder lh in LayerList)
        {
            lh.PropertyChanged -= LayerListItemPropertyChanged;
        }

        LayerList.Clear();

        foreach (CadLayer layer in layerListInfo.LayerList)
        {
            LayerHolder layerHolder = new LayerHolder(layer);
            layerHolder.PropertyChanged += LayerListItemPropertyChanged;

            LayerList.Add(layerHolder);
        }

        int idx = GetLayerListIndex(layerListInfo.CurrentID);
        if (idx >= 0)
        {
            SelectedItem = LayerList[idx];
        }
    }

    private int GetLayerListIndex(uint id)
    {
        int idx = 0;
        foreach (LayerHolder layer in LayerList)
        {
            if (layer.ID == id)
            {
                return idx;
            }
            idx++;
        }

        return -1;
    }
}
