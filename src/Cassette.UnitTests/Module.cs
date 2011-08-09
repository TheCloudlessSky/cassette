﻿using System;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class Module_Tests
    {
        IFileSystem fileSystem = Mock.Of<IFileSystem>();

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingBackSlash()
        {
            var module = new Module("test\\", fileSystem);
            module.Directory.ShouldEqual("test");
        }

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingForwardSlash()
        {
            var module = new Module("test/", fileSystem);
            module.Directory.ShouldEqual("test");
        }

        [Fact]
        public void ContainsPathOfAssetInModule_ReturnsTrue()
        {
            var module = new Module("test", fileSystem);
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithForwardSlash_ReturnsTrue()
        {
            var module = new Module("test", fileSystem);
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("test/asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("test", fileSystem);
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.SourceFilename).Returns("asset.js");
            module.Assets.Add(asset.Object);

            module.ContainsPath("TEST\\ASSET.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetNotInModule_ReturnsFalse()
        {
            var module = new Module("test", fileSystem);

            module.ContainsPath("test\\not-in-module.js").ShouldBeFalse();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItself_ReturnsTrue()
        {
            var module = new Module("test", fileSystem);

            module.ContainsPath("test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("test", fileSystem);

            module.ContainsPath("TEST").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithTrailingSlash_ReturnsTrue()
        {
            var module = new Module("test", fileSystem);

            module.ContainsPath("test\\").ShouldBeTrue();
        }

        [Fact]
        public void AcceptCallsVisitOnVistor()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("test", fileSystem);

            module.Accept(visitor.Object);

            visitor.Verify(v => v.Visit(module));
        }

        [Fact]
        public void AcceptCallsAcceptForEachAsset()
        {
            var visitor = new Mock<IAssetVisitor>();
            var module = new Module("test", fileSystem);
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);
            
            module.Accept(visitor.Object);

            asset1.Verify(a => a.Accept(visitor.Object));
            asset2.Verify(a => a.Accept(visitor.Object));
        }

        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var module = new Module("", fileSystem);
            var asset1 = new Mock<IDisposable>();
            var asset2 = new Mock<IDisposable>();
            var asset3 = new Mock<IAsset>(); // Not disposable; Tests for incorrect casting to IDisposable.
            module.Assets.Add(asset1.As<IAsset>().Object);
            module.Assets.Add(asset2.As<IAsset>().Object);
            module.Assets.Add(asset3.Object);

            module.Dispose();

            asset1.Verify(a => a.Dispose());
            asset2.Verify(a => a.Dispose());
        }
    }
}
