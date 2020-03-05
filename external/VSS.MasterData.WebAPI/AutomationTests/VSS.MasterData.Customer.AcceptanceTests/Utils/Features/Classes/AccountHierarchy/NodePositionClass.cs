using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchy
{
 public class Node
  {

   public Guid NodeUID;

   public  Node left;

   public Node right;

    public Node(Guid item)
    {
      this.NodeUID = item;
      this.right = null;
      this.left = null;
    }
  }
  class BinaryTree
  {

    //  Root of Binary Tree
    Node root;

    BinaryTree()
    {
      this.root = null;
    }


    void printPreorder(Node node)
    {
      if ((node == null))
      {
        return;
      }

      //System.out.print((node.key + " "));
      this.printPreorder(node.left);
      this.printPreorder(node.right);
    }
  }
}
