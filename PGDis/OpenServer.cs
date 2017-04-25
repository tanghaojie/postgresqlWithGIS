using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GISClient;
using System;
using System.Windows.Forms;

namespace PGDis
{
    class OpenServer
    {
        private IMapServer mapserver;
        public IMapServer MpServer
        {
            get
            {
                return this.mapserver;
            }

        }


        /// <summary>
        /// 获取ArcGISServer服务图层
        /// </summary>
        /// <param name="serverUrl"></param>服务器地址
        /// <param name="mapservername"></param>地图服务名称
        /// <param name="isLan"></param>地图服务是局域网内还是互联网上的
        /// <returns></returns>
        public ILayer GetServerLyr(String serverUrl, String mapservername, bool isLan)
        {
            ILayer lyr = null;
            //获得服务对象名称
            try
            {
                IAGSServerObjectName pServerObjectName = GetAGSMapServer(serverUrl, mapservername, isLan);//获取地图
                IName pName = (IName)pServerObjectName;
                //访问地图服务
                IAGSServerObject pServerObject = (IAGSServerObject)pName.Open();
                IMapServer pMapServer = (IMapServer)pServerObject;
                mapserver = pMapServer;//获取地图服务对象

                IMapServerLayer pMapServerLayer = new MapServerLayer() as IMapServerLayer;
                //连接地图服务
                pMapServerLayer.ServerConnect(pServerObjectName, pMapServer.DefaultMapName);
                //添加数据图层
                lyr = pMapServerLayer as ILayer;
            }
            catch (Exception ex)
            {
                MessageBox.Show("服务器配置信息错误，请到“系统管理-系统配置”设置", "连接失败");
            }

            return lyr;
        }
        /// <summary>
        /// 获取ArcGisServer地图服务标识,连接服务器
        /// </summary>
        /// <param name="pHostOrUrl"></param>服务器主机URL
        /// <param name="pServiceName"></param>服务名称
        /// <param name="pIsLAN"></param>主机是否是在局域网或者是互联网
        /// <returns></returns>
        private IAGSServerObjectName GetAGSMapServer(string pHostOrUrl, string pServiceName, bool pIsLAN)
        {
            //设置连接属性
            IPropertySet pPropertySet = new PropertySet();
            if (pIsLAN)
                pPropertySet.SetProperty("machine", pHostOrUrl);
            else
                pPropertySet.SetProperty("url", pHostOrUrl);

            IAGSServerConnectionFactory pFactory = new AGSServerConnectionFactory();
            IAGSServerConnection agsConn = pFactory.Open(pPropertySet, 0);

            //打开获取服务器
            IAGSEnumServerObjectName pServerObjectNames = agsConn.ServerObjectNames;

            //获取服务器上所有服务的标识属性,即服务标识集合
            pServerObjectNames.Reset();
            //使指针指向服务开头
            IAGSServerObjectName ServerObjectName = null;
            //获取服务标识
            while ((ServerObjectName = pServerObjectNames.Next()) != null)
            {
                if ((ServerObjectName.Name.ToLower() == pServiceName.ToLower()) &&
                    (ServerObjectName.Type == "MapServer"))
                {//判断获取所需服务
                    break;
                }
            }
            //返回对象
            return ServerObjectName;//返回服务标识
        }


        public ILayer GetWmsMapServer(string url, string serviceName)
        {
            IPropertySet pPropertyset = new PropertySetClass();
            pPropertyset.SetProperty("url", url);

            IWMSConnectionName pWmsConnectionName = new WMSConnectionNameClass();
            pWmsConnectionName.ConnectionProperties = pPropertyset;
            IWMSConnectionFactory wmsConnFac = new WMSConnectionFactoryClass();
            IName   nn=wmsConnFac.Open(pPropertyset, 0, null) as IName;

            IWMSGroupLayer pWmsMapLayer = new WMSMapLayerClass();
            IDataLayer pDataLayer = pWmsMapLayer as IDataLayer;
            pDataLayer.Connect(pWmsConnectionName as IName);

            IWMSServiceDescription pWmsServiceDesc = pWmsMapLayer.WMSServiceDescription;

            for (int i = 0; i < pWmsServiceDesc.LayerDescriptionCount; i++)
            {
                IWMSLayerDescription pWmsLayerDesc = pWmsServiceDesc.get_LayerDescription(i);
                ILayer pNewLayer = null;
                if (pWmsLayerDesc.LayerDescriptionCount == 0)
                {
                    IWMSLayer pWmsLayer = pWmsMapLayer.CreateWMSLayer(pWmsLayerDesc);
                    pNewLayer = pWmsLayer as ILayer;
                }
                else
                {
                    IWMSGroupLayer pWmsGroupLayer = pWmsMapLayer.CreateWMSGroupLayers(pWmsLayerDesc);
                    for (int j = 0; j < pWmsGroupLayer.Count; j++)
                    {
                        ILayer layer = pWmsGroupLayer.get_Layer(j);
                        if (layer.Name.ToLower() == serviceName.ToLower())
                        {
                            pWmsMapLayer.InsertLayer(layer, 0);
                            layer.Visible = true;
                            break;
                        }
                    }

                }
            }
            ILayer pLayer = pWmsMapLayer as ILayer;
            pLayer.Name = pWmsServiceDesc.WMSTitle;
            pLayer.Visible = true;
            return pLayer;
        }


    }
}



