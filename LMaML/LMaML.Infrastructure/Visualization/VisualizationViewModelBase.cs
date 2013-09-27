using System;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LMaML.Infrastructure.Behaviours;
using LMaML.Infrastructure.Events;
using LMaML.Infrastructure.Services.Interfaces;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Common.WPF;
using iLynx.Common.WPF.Imaging;

namespace LMaML.Infrastructure.Visualization
{
    public abstract class VisualizationViewModelBase : NotificationBase, IVisualization
    {
        private readonly IThreadManager threadManager;
        protected readonly IPlayerService PlayerService;
        private readonly IDispatcher dispatcher;
        private UnmanagedBitmapRenderer renderer;
        private double renderHeight;
        private double renderWidth;
        protected readonly object SyncRoot = new object();
        private readonly Timer changeTimer;
        private bool isResizing;
        private bool lastRunning;
        private ICommand sizeChangedCommand;
        private BitmapSource source;

        /// <summary>
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="threadManager">The thread manager.</param>
        /// <param name="playerService">The player service.</param>
        /// <param name="publicTransport">The public transport.</param>
        /// <param name="dispatcher">The dispatcher.</param>
        protected VisualizationViewModelBase(ILogger logger, IThreadManager threadManager, IPlayerService playerService, IPublicTransport publicTransport, IDispatcher dispatcher)
            : base(logger)
        {
            this.threadManager = threadManager;
            PlayerService = playerService;
            this.dispatcher = dispatcher;
            publicTransport.ApplicationEventBus.Subscribe<PlayingStateChangedEvent>(OnPlayingStateChanged);
            publicTransport.ApplicationEventBus.Subscribe<ShellCollapsedEvent>(OnShellCollapsed);
            publicTransport.ApplicationEventBus.Subscribe<ShellExpandedEvent>(OnShellExpanded);
            publicTransport.ApplicationEventBus.Subscribe<ShellResizeBeginEvent>(OnShellResizeBegin);
            publicTransport.ApplicationEventBus.Subscribe<ShellResizeEndEvent>(OnShellResizeEnd);
            changeTimer = new Timer(SizeChanged);
        }

        public ICommand SizeChangedCommand
        {
            get { return sizeChangedCommand ?? (sizeChangedCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand<SizeContainer>(OnSizeChanged)); }
        }

        /// <summary>
        /// Gets or sets the height of the render.
        /// </summary>
        /// <value>
        /// The height of the render.
        /// </value>
        public double RenderHeight
        {
            set
            {
                if (Math.Abs(value - renderHeight) <= double.Epsilon) return;
                renderHeight = value;
                if (Math.Abs(renderWidth) <= double.Epsilon) return;
                changeTimer.Change(250, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Gets the stretch.
        /// </summary>
        /// <value>
        /// The stretch.
        /// </value>
        public Stretch Stretch { get { return Stretch.Fill; } }

        /// <summary>
        /// Gets or sets the width of the render.
        /// </summary>
        /// <value>
        /// The width of the render.
        /// </value>
        public double RenderWidth
        {
            set
            {
                if (Math.Abs(value - renderWidth) <= double.Epsilon) return;
                renderWidth = value;
                if (Math.Abs(renderHeight) <= double.Epsilon) return;
                //Recreate();
                changeTimer.Change(250, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        public BitmapSource Image
        {
            get { return source; }
            set
            {
                if (ReferenceEquals(value, source)) return;
                source = value;
                RaisePropertyChanged(() => Image);
            }
        }

        private void OnShellResizeEnd(ShellResizeEndEvent shellResizeEndEvent)
        {
            isResizing = false;
            changeTimer.Change(250, Timeout.Infinite);
        }

        private void OnShellResizeBegin(ShellResizeBeginEvent shellResizeBeginEvent)
        {
            changeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            isResizing = true;
        }

        private void OnShellExpanded(ShellExpandedEvent shellExpandedEvent)
        {
            if (lastRunning)
                renderer.Start();
        }

        private void OnShellCollapsed(ShellCollapsedEvent shellCollapsedEvent)
        {
            lastRunning = renderer.IsRunning;
            renderer.Stop();
        }

        private void OnPlayingStateChanged(PlayingStateChangedEvent playingStateChangedEvent)
        {
            if (null == renderer)
                return;
            
            if (playingStateChangedEvent.NewState != PlayingState.Playing)
                renderer.Stop();
            else if (!renderer.IsRunning)
                renderer.Start();
        }

        /// <summary>
        /// Sizes the changed.
        /// </summary>
        /// <param name="state">The state.</param>
        private void SizeChanged(object state)
        {
            if (isResizing) return;
            changeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Recreate();
        }

        /// <summary>
        /// Called when [size changed].
        /// </summary>
        /// <param name="size">The size.</param>
        private void OnSizeChanged(SizeContainer size)
        {
            RenderHeight = size.Height;
            RenderWidth = size.Width;
        }

        /// <summary>
        /// Recreates this instance.
        /// </summary>
        private void Recreate()
        {
            var width = (int)renderWidth;
            var height = (int)renderHeight;
            if (null != renderer)
                renderer.Stop();
            renderer = Create(width, height);
            renderer.SourceCreated += RendererOnSourceCreated;
            if (PlayerService.State != PlayingState.Playing) return;
            renderer.Start();
        }

        private UnmanagedBitmapRenderer Create(int width, int height)
        {
            var r = new UnmanagedBitmapRenderer(threadManager, dispatcher, width, height, ((width * 32) + 7) / 8) { ClearEachPass = true, DesiredFramerate = 60 };
            r.RegisterRenderCallback(Render, 0);
            return r;
        }

        /// <summary>
        /// Renderers the on source created.
        /// </summary>
        /// <param name="bitmapSource">The bitmap source.</param>
        private void RendererOnSourceCreated(BitmapSource bitmapSource)
        {
            dispatcher.Invoke(src => Image = src, bitmapSource);
        }

        /// <summary>
        /// Renders the callback.
        /// </summary>
        /// <param name="backBuffer">The back buffer.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="stride">The stride.</param>
        protected abstract void Render(IntPtr backBuffer, int width, int height, int stride);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (null == renderer)
            {
                return;
            }
            renderer.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            renderer.Stop();
        }
    }
}