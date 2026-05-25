using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaView.Core;
using MetaView.Core.ModulesInterface;

namespace MetaView.Services.Interfaces
{
    public class AbstractTaskServices
    {
        private readonly  IDaqAndPreprocessing _daqAndPreprocessing;
        private readonly IImageAcquisition _imageAcquisition;
        private readonly IMotionControl _linearMotionMotion;
        

        public AbstractTaskServices()
        {
        }

        /// <summary>
        /// 主动对焦
        /// </summary>
        /// <returns></returns>
        async Task<bool> AutoFocus()
        {
            return true;
        }

        /// <summary>
        /// 获取1维光谱数据
        /// </summary>
        /// <returns></returns>
        async Task<bool> Get1DPlot()
        {
            return true;
        }

        /// <summary>
        /// 获取2D光谱图
        /// </summary>
        /// <returns></returns>
        async Task<bool> Get2DImage()
        {
            return true;
        }


        /// <summary>
        /// 获取3维光谱数据
        /// </summary>
        /// <returns></returns>
        async Task<bool> Get3DData()
        {
            return true;
        }

        /// <summary>
        /// 获取4维光谱图
        /// </summary>
        /// <returns></returns>
        async Task<bool> Get4DData()
        { return true; }

    }
}
