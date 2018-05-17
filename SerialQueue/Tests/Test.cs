﻿using NUnit.Framework;
using System;
using Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Tests
{
    [TestFixture()]
    public class Test
    {
        [Test]
        public async Task TplIsNotFifo()
        {
            // Assign

            var list = new List<int>();
            var tasks = new List<Task>();
            var range = Enumerable.Range(0, 100000);

            // Act

            foreach (var number in range) {
                tasks.Add(Task.Factory.StartNew(() => list.Add(number), TaskCreationOptions.PreferFairness));
            }

            await Task.WhenAll(tasks);

            // Assert

            Assert.False(range.SequenceEqual(list));
        }

        [Test]
        public async Task QueueAction()
        {
            // Assign

            var queue = new SerialQueue();
            var list = new List<int>();
            var tasks = new List<Task>();
            var range = Enumerable.Range(0, 10000);

            // Act

            foreach (var number in range) {
                tasks.Add(queue.Enqueue(() => list.Add(number)));
            }

            await Task.WhenAll(tasks);

            // Assert

            Assert.True(range.SequenceEqual(list));
        }

        [Test]
        public async Task QueueFunction()
        {
            // Assign

            var queue = new SerialQueue();
            var list = new List<int>();
            var tasks = new List<Task<int>>();
            var range = Enumerable.Range(0, 10000);

            // Act

            foreach (var number in range) {
                tasks.Add(queue.Enqueue(() => {
                    list.Add(number);
                    return number;
                }));
            }

            await Task.WhenAll(tasks);

            // Assert

            Assert.True(tasks.Select(x => x.Result).SequenceEqual(list));
        }

        [Test]
        public void EnqueueFromMultipleThreads()
        {
            // Assign

            const int count = 10000;
            var queue = new SerialQueue();
            var list = new List<int>();

            // Act

            var counter = 0;
            for (int i = 0; i < count; i++) {
                Task.Run(() => {
                    queue.Enqueue(() => list.Add(counter++));
                });
            }

            while (list.Count != count) {};

            // Assert

            Assert.True(list.SequenceEqual(Enumerable.Range(0, count)));
        }
    }
}

