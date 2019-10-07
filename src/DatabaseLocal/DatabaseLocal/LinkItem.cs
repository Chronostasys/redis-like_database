using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseLocal
{
    class LinkItem<T>:IDisposable
    {
        public T t;
        public LinkItem<T> prev;
        public LinkItem<T> next;
        public LinkItem()
        {

        }

        public void Dispose()
        {
            prev = null;
            next = null;
        }
    }
    class LinkList<T>
    {
        public virtual T this[int i]
        {
            get 
            {
                T t=first.t;
                LinkItem<T> l;
                l = first;
                for (int a = 0; a < i; a++)
                {
                    l = l.next;
                    t = l.t;
                }
                return t;
            }
            set
            {
                LinkItem<T> l;
                l = first;
                for (int a = 0; a < i-1; a++)
                {
                    l = l.next;
                }
                l.t = value;
            }
        }
        int Length;
        LinkItem<T> first;
        LinkItem<T> Last;
        public LinkList()
        {
            first = new LinkItem<T>();
            Last = new LinkItem<T>();
            Length = 0;
        }
        public void Add(T value)
        {
            LinkItem<T> l;
            l = first;
            if (Length==0)
            {
                first.t = value;
            }
            else
            {
                for (int a = 0; a < Length-1; a++)
                {
                    l = l.next;
                }
                l.next = new LinkItem<T>() { prev = l, t = value, next = null };
            }
            Length++;
        }
        public LinkItem<T> GetLinkItem(int index)
        {
            LinkItem<T> l;
            l = first;
            if (Length != 0)
            {
                for (int a = 0; a < index; a++)
                {
                    l = l.next;
                }
            }
            return l;
        }
        public void Remove(int index)
        {
            var del = GetLinkItem(index);
            var next = GetLinkItem(index + 1);
            if (index!=0)
            {
                var pre = GetLinkItem(index - 1);
                pre.next = next;
                next.prev = pre;
            }
            else
            {
                next.prev = null;
                first = next;
            }
            del.Dispose();
        }
        public void Insert(int index, T val)
        {
            var next = GetLinkItem(index);
            if (index==0)
            {
                first = new LinkItem<T>() { next = next, prev = null, t = val };
            }
            else
            {
                var prev = GetLinkItem(index - 1);
                var ins = new LinkItem<T> { next = next, prev = prev, t = val };
                prev.next = ins;
                next.prev = ins;
            }
        }
    }
}
