using System;
using System.Collections.Generic;
using System.Windows;
using ArchiveViewer.UserControls.Home;
using ArchiveViewer.UserControls.Option;
using ArchiveViewer.UserControls.Shell;
using Caliburn.Micro;

namespace ArchiveViewer
{
    public class ArchiveBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public ArchiveBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();
            _container.RegisterInstance(typeof(SimpleContainer), null, _container);
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IWindowManager, WindowManager>();


            _container.Singleton<ShellViewModel>();
            {
                _container.Singleton<HomeViewModel>();
                _container.Singleton<OptionViewModel>();
            }
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}