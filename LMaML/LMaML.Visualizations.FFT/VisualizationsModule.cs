﻿using LMaML.Infrastructure;
using LMaML.Infrastructure.Services.Interfaces;
using LMaML.Visualizations.FFT.ViewModels;
using Microsoft.Practices.Unity;

namespace LMaML.Visualizations.FFT
{
    public class VisualizationsModule : ModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationsModule" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public VisualizationsModule(IUnityContainer container) : base(container)
        {
        }

        /// <summary>
        /// Registers the types.
        /// <para>
        /// This is the second method called in the initialization process (Called AFTER AddResources)
        /// </para>
        /// </summary>
        protected override void RegisterTypes()
        {
            var registry = Container.Resolve<IVisualizationRegistry>();
            registry.Register(() => Container.Resolve<SimpleFFTVisualizationViewModel>(), "Simple FFT");
            registry.Register(() => Container.Resolve<SpectralFFTVisualizationViewModel>(), "\"Spectral\" FFT");
        }
    }
}
