using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMArmDLL
{
	public class Tree
	{
		public struct Node
		{
			public int leftChild, rightChild, parent;
			public bool isEmpty;
			public double q, sin, cos;
		}
		//public double[,] treeArray;
		public Node[] node;
		public int nodeNum;
		public Tree(double rootNode)
		{
			this.node = new Node[256];
			for (int i = 0; i < 256; i++)
			{
				this.node[i].q = 0;//初始化所有预设节点的值
				this.node[i].sin = 0;
				this.node[i].cos = 0;
				this.node[i].leftChild = -1;
				this.node[i].rightChild = -1;
				this.node[i].parent = -1;//所有节点的左右孩子、父节点均为空(-1)
				this.node[i].isEmpty = true;
			}
			node[0].q = rootNode;//设置根节点的值
			node[0].sin = Math.Sin(rootNode);
			node[0].cos = Math.Cos(rootNode);
			node[0].isEmpty = false;//根节点非空
			nodeNum = 1;
		}
		public double[,] arrayOfTree()
		{
			double[,] treeArray = new double[nodeNum, 6];
			for (int i = 0; i < nodeNum; i++)
			{
				treeArray[i, 0] = node[i].leftChild;
				treeArray[i, 1] = node[i].rightChild;
				treeArray[i, 2] = node[i].parent;
				treeArray[i, 3] = node[i].q;
				treeArray[i, 4] = node[i].sin;
				treeArray[i, 5] = node[i].cos;
			}
			return treeArray;
		}
		/// <summary>
		/// 加入一个新节点,如果父节点左空则置入左,左非空右空则置入右,否则返回错误值-1
		/// </summary>
		/// <param name="fatherNode">父节点的索引编号</param>
		/// <param name="nodeValue">要置入的节点的数据</param>
		/// <returns>返回置入后的节点的索引编号,如果无法置入则返回-1</returns>
		public int addNode(int fatherNode, double nodeValue)
		{
			int newNode = -1;//新节点的索引编号
			for (int i = 0; i < 256; i++)
			{
				if (node[i].isEmpty)//如果有空节点，则将值塞入空节点
				{
					node[i].q = nodeValue;
					node[i].sin = Math.Sin(nodeValue);
					node[i].cos = Math.Cos(nodeValue);
					node[i].isEmpty = false;//节点非空
					newNode = i;//此时i为新节点的索引编号
					break;
				}
			}
			if (node[fatherNode].leftChild == -1)//左孩子为空,则添加为左孩子
			{
				node[fatherNode].leftChild = newNode;//添加到左孩子
				node[newNode].parent = fatherNode;
				nodeNum++;
			}
			else if (node[fatherNode].rightChild == -1)//如果左孩子非空且右孩子为空,则添加到右孩子
			{
				node[fatherNode].rightChild = newNode;
				node[newNode].parent = fatherNode;
				nodeNum++;
			}
			else
			{
				return -1;
			}
			return newNode;//返回新节点的索引值
		}
		/// <summary>
		/// 获取一个节点的父节点
		/// </summary>
		/// <param name="nodeIndex"></param>
		/// <returns></returns>
		public int getParent(int nodeIndex)
		{
			return this.node[nodeIndex].parent;
		}
		/// <summary>
		/// 获取一个节点第generation代父节点,generation=0时返回自身,=1时为父节点,=2时为父节点的父节点,以此类推
		/// </summary>
		/// <param name="nodeIndex"></param>
		/// <param name="generation"></param>
		/// <returns></returns>
		public int getParent(int nodeIndex, int generation)
		{
			int parentIndex = nodeIndex;
			for (int i = 0; i < generation; i++)
			{
				parentIndex = this.node[parentIndex].parent;
			}
			return parentIndex;
		}
		/// <summary>
		/// 获取一个节点的左子节点
		/// </summary>
		/// <param name="nodeIndex"></param>
		/// <returns></returns>
		public int getLeftChild(int nodeIndex)
		{
			return this.node[nodeIndex].leftChild;
		}
		/// <summary>
		/// 获取一个节点的右子节点
		/// </summary>
		/// <param name="nodeIndex"></param>
		/// <returns></returns>
		public int getRightChild(int nodeIndex)
		{
			return this.node[nodeIndex].rightChild;
		}
	}
}
