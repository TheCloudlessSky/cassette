﻿using System;
using System.Collections.Generic;

namespace Cassette
{
    public class CassetteApplication : ICassetteApplication
    {
        public CassetteApplication(IFileSystem sourceFileSystem, IFileSystem cacheFileSystem)
        {
            this.sourceFileSystem = sourceFileSystem;
            this.cacheFileSystem = cacheFileSystem;
        }

        readonly IFileSystem sourceFileSystem;
        readonly IFileSystem cacheFileSystem;
        readonly List<Action> initializers = new List<Action>();
        readonly Dictionary<Type, object> moduleContainers = new Dictionary<Type, object>();

        public IFileSystem RootDirectory
        {
            get { return sourceFileSystem; }
        }

        public IModuleCache<T> GetModuleCache<T>()
            where T : Module
        {
            return new ModuleCache<T>(
                cacheFileSystem.AtSubDirectory(typeof(T).Name, true),
                GetModuleFactory<T>()
            );
        }

        public virtual IModuleFactory<T> GetModuleFactory<T>()
            where T : Module
        {
            if (typeof(T) == typeof(ScriptModule))
            {
                return (IModuleFactory<T>)new ScriptModuleFactory(RootDirectory);
            }
            if (typeof(T) == typeof(StylesheetModule))
            {
                return (IModuleFactory<T>)new StylesheetModuleFactory(RootDirectory);
            }
            if (typeof(T) == typeof(HtmlTemplateModule))
            {
                return (IModuleFactory<T>)new HtmlTemplateModuleFactory(RootDirectory);
            }
            throw new NotSupportedException("Cannot find the factory for " + typeof(T).FullName + ".");
        }

        public IModuleContainer<T> GetModuleContainer<T>()
            where T: Module
        {
            // TODO: Throw better exception when module of type T is not defined.
            return (IModuleContainer<T>)moduleContainers[typeof(T)];
        }

        public void AddModuleContainerFactory<T>(IModuleContainerFactory<T> moduleContainerFactory)
            where T : Module
        {
            initializers.Add(() =>
            {
                var container = moduleContainerFactory.CreateModuleContainer();
                moduleContainers[typeof(T)] = container;
            });
        }

        public void InitializeModuleContainers()
        {
            foreach (var initializer in initializers)
            {
                initializer();
            }
            initializers.Clear();
        }

    }
}
