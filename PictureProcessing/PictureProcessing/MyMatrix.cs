using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureProcessing
{
    // 声明：MyMatrix 部分功能实现思路参考了互联网相关资料，具体实现为笔者自行完成，相关部分已用 * 号标注，未使用 * 标注为完全自主完成。
    class MyMatrix
    {
        // 变量存储区
        private readonly double[,] _matrix_data;

        private readonly int _row;

        private readonly int _column;

        private readonly bool _square;

        // 数据接口
        public int Row => _row;

        public int Column => _column;

        public bool Square => _square;

        public double[,] GetMatrix_data()
        {
            return _matrix_data;
        }

        // 构造函数
        public MyMatrix(double[][] matrix_data)
        {
            _row = matrix_data.Length;
            _column = matrix_data[0].Length;
            double[,] processData = new double[this.Row, this.Column];
            for (int i = 0; i < this.Row; i++)
            {
                for (int j = 0; j < this.Column; j++)
                {
                    processData[i, j] = matrix_data[i][j];
                }
            }
            _matrix_data = processData;
            _square = (this.Row == this.Column) && (this.GetMatrix_data() != null);
        }

        public MyMatrix(double[,] matrix_data, int row, int column)
        {
            _matrix_data = matrix_data ?? throw new ArgumentNullException(nameof(matrix_data));
            _row = row;
            _column = column;
            _square = (this.Row == this.Column) && (this.GetMatrix_data() != null);
        }

        public MyMatrix(MyMatrix sourceMatrix)
        {
            _matrix_data = sourceMatrix.GetMatrix_data();
            _row = sourceMatrix.Row;
            _column = sourceMatrix.Column;
            _square = (this.Row == this.Column) && (this.GetMatrix_data() != null);
        }

        // 转置运算
        public MyMatrix Transpose()
        {
            double[,] processData = new double[this.Column, this.Row];
            for (int i = 0; i < this.Row; i++)
            {
                for (int j = 0; j < this.Column; j++)
                {
                    processData[j, i] = this.GetMatrix_data()[i,j];
                }
            }
            return new MyMatrix(processData, this.Column, this.Row);
        }

        // 矩阵乘法
        public MyMatrix Multiply(MyMatrix myMatrix)
        {
            if(this.Column==myMatrix.Row)
            {
                double[,] processData = new double[this.Row, myMatrix.Column];
                for (int i = 0; i < this.Row; i++)
                {
                    for (int j = 0; j < myMatrix.Column; j++)
                    {
                        processData[i, j] = 0;
                        for(int m = 0; m < this.Column;m++)
                        {
                            processData[i, j] += this.GetMatrix_data()[i, m] * myMatrix.GetMatrix_data()[m, j];
                        }
                    }
                }
                return new MyMatrix(processData, this.Row, myMatrix.Column);
            }
            else
            {
                return null;
            }
        }

        // * 矩阵求逆
        public MyMatrix Inverse()
        {
            // 非方阵，退出
            if (!this.Square) 
            { 
                return null; 
            }

            // 矩阵备份
            double[,] processData = new double[this.Row, this.Column];
            for (int i=0;i<this.Row;i++)            
            {
                for(int j=0;j<this.Column;j++)                
                {                    
                    processData[i, j] = this.GetMatrix_data()[i, j];                
                }            
            }          
            
            // 单位矩阵
            double[,] eye = new double[this.Row, this.Column];
            for (int i = 0; i < this.Row; i++)            
            {                
                for (int j = 0; j < this.Column; j++)                
                {                    
                    if (i == j) 
                    { 
                        eye[i, j] = 1; 
                    }                    
                    else 
                    { 
                        eye[i, j] = 0; 
                    }                
                }            
            }
            
            // 矩阵处理
            for(int j=0;j< this.Column; j++)            
            {                
                bool flag=false;

                for(int i=j;i< this.Row; i++)                
                {                    
                    if(processData[i,j]!=0)                    
                    {                        
                        flag = true;                        
                        double temp;           
                        if (i != j)                        
                        {                            
                            for (int k = 0; k < this.Column; k++)                            
                            {                                
                                temp = processData[j, k];
                                processData[j, k] = processData[i, k];                                
                                processData[i, k] = temp;                                 
                                temp = eye[j, k];                                
                                eye[j, k] = eye[i, k];                                
                                eye[i, k] = temp;                            
                            }                        
                        }

                        double d = processData[j, j];                        
                        for(int k=0;k<this.Column;k++)                        
                        {                            
                            processData[j,k]=processData[j,k]/d;                            
                            eye[j, k]= eye[j,k]/d;                       
                        }
                        
                        d = processData[j, j];                        
                        for(int k=0;k<this.Row;k++)                        
                        {                            
                            if(k!=j)                            
                            {                                
                                double t = processData[k, j];                                
                                for(int n=0;n<this.Column;n++)                                
                                {                                    
                                    processData[k,n]-=(t/d)*processData[j,n];                                    
                                    eye[k,n]-=(t/d)*eye[j,n];                                
                                }                            
                            }                        
                        }                    
                    }                
                }

                if (!flag)
                {
                    return null;
                }
            }
            return new MyMatrix(eye, this.Row, this.Column);
        }
    }
}
