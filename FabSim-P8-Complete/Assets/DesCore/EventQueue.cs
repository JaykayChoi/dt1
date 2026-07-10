using System;
using System.Collections.Generic;

namespace DesCore
{
    /// <summary>
    /// 미래 사건 목록(FEL, Future Event List) — (시각, 일련번호) 오름차순으로
    /// 사건을 꺼내는 이진 최소 힙. Unity의 .NET 프로파일에는 PriorityQueue가
    /// 없으므로 직접 구현한다.
    /// </summary>
    public sealed class EventQueue
    {
        private readonly List<SimEvent> heap = new List<SimEvent>();

        /// <summary>대기 중인 사건 수.</summary>
        public int Count => heap.Count;

        /// <summary>대기 중인 사건이 없으면 true.</summary>
        public bool IsEmpty => heap.Count == 0;

        /// <summary>다음에 실행될 사건의 시각. 큐가 비어 있으면 예외.</summary>
        public double PeekTime
        {
            get
            {
                if (heap.Count == 0)
                {
                    throw new InvalidOperationException("Event queue is empty.");
                }

                return heap[0].Time;
            }
        }

        /// <summary>사건을 큐에 넣는다. O(log n).</summary>
        public void Push(SimEvent item)
        {
            heap.Add(item);
            SiftUp(heap.Count - 1);
        }

        /// <summary>가장 이른 사건을 꺼낸다. O(log n).</summary>
        public SimEvent Pop()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("Event queue is empty.");
            }

            SimEvent root = heap[0];
            int lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);
            if (heap.Count > 0)
            {
                SiftDown(0);
            }

            return root;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (!heap[index].IsBefore(heap[parent]))
                {
                    break;
                }

                Swap(index, parent);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                int smallest = index;
                int left = index * 2 + 1;
                int right = index * 2 + 2;
                if (left < heap.Count && heap[left].IsBefore(heap[smallest]))
                {
                    smallest = left;
                }

                if (right < heap.Count && heap[right].IsBefore(heap[smallest]))
                {
                    smallest = right;
                }

                if (smallest == index)
                {
                    break;
                }

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            SimEvent temp = heap[a];
            heap[a] = heap[b];
            heap[b] = temp;
        }
    }
}
