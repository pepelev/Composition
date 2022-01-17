using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Composition.Tests
{
    public sealed class SingletonBagShould
    {
        [Test]
        public void Give_What_Factory_Gives()
        {
            var sut = new SingletonBag();

            var actual = sut.Get(() => 10);

            actual.Should().Be(10);
        }

        [Test]
        public async Task Give_Async_What_Factory_Gives()
        {
            var sut = new SingletonBag();

            var actual = await sut.GetAsync(() => Task.FromResult(10)).ConfigureAwait(false);

            actual.Should().Be(10);
        }

        [Test]
        public void Return_Same_On_Multiple_Calls()
        {
            var sut = new SingletonBag();

            var first = sut.Get(() => new Version(12, 3));
            var second = sut.Get(() => new Version(12, 3));

            second.Should().BeSameAs(first);
        }

        [Test]
        public async Task Return_Same_On_Multiple_Async_Calls()
        {
            var sut = new SingletonBag();

            var first = await sut.GetAsync(() => Task.FromResult(new Version(12, 3))).ConfigureAwait(false);
            var second = await sut.GetAsync(() => Task.FromResult(new Version(12, 3))).ConfigureAwait(false);

            second.Should().BeSameAs(first);
        }

        [Test]
        public void Return_Same_On_Different_Factories()
        {
            var sut = new SingletonBag();

            var first = sut.Get(() => new Version(12, 3));
            var second = sut.Get(() => new Version(13, 20));

            second.Should().BeSameAs(first);
        }

        [Test]
        public async Task Return_Same_On_Different_Async_Factories()
        {
            var sut = new SingletonBag();

            var first = await sut.GetAsync(
                () => Task.FromResult(new Version(12, 3))
            ).ConfigureAwait(false);
            var second = await sut.GetAsync(
                () => Task.FromResult(new Version(13, 20))
            ).ConfigureAwait(false);

            second.Should().BeSameAs(first);
        }

        [Test]
        public void Call_Factory_Once()
        {
            var sut = new SingletonBag();
            var factory = Substitute.For<Func<string>>();

            sut.Get(factory);
            sut.Get(factory);

            factory.Received(1).Invoke();
        }

        [Test]
        public async Task Call_Async_Factory_Once()
        {
            var sut = new SingletonBag();
            var factory = Substitute.For<Func<Task<string>>>();

            await sut.GetAsync(factory).ConfigureAwait(false);
            await sut.GetAsync(factory).ConfigureAwait(false);

            await factory.Received(1).Invoke().ConfigureAwait(false);
        }

        [Test]
        public void Distinguish_By_Method_Name()
        {
            var sut = new SingletonBag();

            (A(sut), B(sut)).Should().Be((11, 35));
        }

        [Test]
        public async Task Distinguish_By_Method_Name_For_Async()
        {
            var sut = new SingletonBag();

            var a = await AAsync(sut).ConfigureAwait(false);
            var b = await BAsync(sut).ConfigureAwait(false);
            (a, b).Should().Be((11, 35));
        }

        [Test]
        public void Distinguish_By_Type()
        {
            var sut = new SingletonBag();

            var a = sut.Get(() => 17);
            var b = sut.Get(() => "Hello");

            (a, b).Should().Be((17, "Hello"));
        }

        [Test]
        public async Task Distinguish_By_Type_For_Async()
        {
            var sut = new SingletonBag();

            var a = await sut.GetAsync(() => Task.FromResult(17)).ConfigureAwait(false);
            var b = await sut.GetAsync(() => Task.FromResult("Hello")).ConfigureAwait(false);

            (a, b).Should().Be((17, "Hello"));
        }

        [Test]
        public void Not_Cache_Exception()
        {
            var sut = new SingletonBag();
            var factory = Substitute.For<Func<string>>();
            var exception = new Exception("fail");
            factory.Invoke().Returns(
                _ => throw exception,
                _ => "result"
            );

            var actualException = Assert.Throws<Exception>(() => sut.Get(factory));
            actualException.Should().BeSameAs(exception);

            var actual = sut.Get(factory);

            actual.Should().Be("result");
        }

        [Test]
        public async Task Not_Cache_Exception_In_Async()
        {
            var sut = new SingletonBag();
            var factory = Substitute.For<Func<Task<string>>>();
            var exception = new Exception("fail");
            factory.Invoke().Returns(
                _ => throw exception,
                _ => "result"
            );

            var actualException = Assert.Throws<Exception>(() => sut.Get(factory));
            actualException.Should().BeSameAs(exception);

            var actual = await sut.GetAsync(factory).ConfigureAwait(false);

            actual.Should().Be("result");
        }

        private static int A(SingletonBag bag) => bag.Get(() => 11);
        private static int B(SingletonBag bag) => bag.Get(() => 35);

        private static async Task<int> AAsync(SingletonBag bag) =>
            await bag.GetAsync(() => Task.FromResult(11)).ConfigureAwait(false);

        private static async Task<int> BAsync(SingletonBag bag) =>
            await bag.GetAsync(() => Task.FromResult(35)).ConfigureAwait(false);
    }
}