public struct Node<T>
{
    public struct Link
    {
        public Node<T> Node0{ get; private set; }
        public Node<T> Node1{ get; private set; }
        public Link(Node<T> node0, Node<T> node1)
        {
            this.Node0 = node0;
            this.Node1 = node1;
            this.Node0.Links.Add(this);
            this.Node1.Links.Add(this);
        }
        public void Unlink()
        {
            this.Node0.Links.Remove(this);
            this.Node1.Links.Remove(this);
        }
    }

    public T Data;
    public HashSet<Link> Links { get; private set; }

    public Node(T data)
    {
        this.Data = data;
        this.Links = new HashSet<Link>();
    }
}
