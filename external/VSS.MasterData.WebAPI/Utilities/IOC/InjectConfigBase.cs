using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.IOC
{
	public abstract class InjectConfigBase : IInjectConfig
    {
        protected IServiceCollection _serviceCollection;
		private IServiceProvider _serviceProvider;
		protected virtual void PreConfigureServices() { }
        public abstract void ConfigureServiceCollection(IServiceCollection services);
        protected virtual void PostConfigureServices() { }
        public void Dispose()
        {
			this._serviceProvider = null;
			this._serviceCollection = null;
		}
        public virtual T Resolve<T>()
        {
            return _serviceProvider.GetService<T>();
        }

		public virtual IEnumerable<T> Resolves<T>()
		{
			return _serviceProvider.GetServices<T>();
		}

		public virtual TInterface ResolveKeyed<TInterface>(string name)
        {
            return _serviceProvider.GetServices<TInterface>().First(x => x.GetType().Name.Equals(name));
        }

		public virtual TInterface ConfigureServiceContainer<TInterface>()
        {
            this.ConfigureContainer();
            return this.Resolve<TInterface>();
        }

        public virtual void ConfigureServiceContainer()
        {
            this.ConfigureContainer();
        }

        public IServiceProvider GetContainer(IServiceCollection services)
        {
			if (this._serviceCollection == null)
			{
				this._serviceCollection = services;
			}
            if (this._serviceProvider == null)
            {
                this.ConfigureContainer();
            }
            return this._serviceProvider;
        }

        private void ConfigureContainer()
        {
			this.PreConfigureServices();
            this.ConfigureServiceCollection(this._serviceCollection);
            this.PostConfigureServices();
			_serviceProvider = _serviceCollection.BuildServiceProvider();
        }

		public void AddScopedService<TInterface, TImplementation>(object singletonObject)
		{
			throw new NotImplementedException();
		}

		public void AddTransientService<TInterface, TImplementation>(object singletonObject)
		{
			throw new NotImplementedException();
		}

		public IServiceProvider BuildServiceProvider()
		{
			this._serviceProvider = this._serviceCollection.BuildServiceProvider();
			return this._serviceProvider;
		}

		public void AddSingletonService<TInterface, TImplementationInstance>(TImplementationInstance singletonObject) where TImplementationInstance : class, TInterface
		{
			throw new NotImplementedException();
		}
	}
}
