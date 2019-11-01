using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureProcessing
{
    // 声明：FaceTransformation 部分功能实现思路参考了互联网相关资料，具体实现为笔者自行完成，相关部分已用 * 号标注，未使用 * 标注为完全自主完成。
    class FaceTransformation
    {
        // 变量存储区
        private readonly ImageIO _source;

        private readonly ImageIO _source_marked;

        private readonly ImageIO _face;

        private readonly ImageIO _face_marked;

        private readonly double[] _source_points;

        private readonly double[] _face_points;

        private readonly string _source_path;

        private readonly string _source_points_path;

        private readonly string _face_path;

        private readonly string _face_points_path;

        private readonly MyMatrix _TPS_transform_matrix;

        private readonly double[] _facePointsAffined;

        // 数据接口
        public double[] Source_points => _source_points;

        public double[] Face_points => _face_points;

        public string Source_path => _source_path;

        public string Source_points_path => _source_points_path;

        public string Face_path => _face_path;

        public string Face_points_path => _face_points_path;

        internal ImageIO Source => _source;

        internal ImageIO Source_marked => _source_marked;

        internal ImageIO Face => _face;

        internal ImageIO Face_marked => _face_marked;

        internal MyMatrix TPS_transform_matrix => _TPS_transform_matrix;

        public double[] FacePointsAffined => _facePointsAffined;

        // 构造函数
        public FaceTransformation(string source_path, string face_path)
        {
            _source_path = source_path;
            _source_points_path = source_path.Split('.')[0] + ".txt";
            _source = new ImageIO(source_path);
            _source.LockBits();
            _source_points = this.LoadPoints(this.Source_points_path);
            _source_marked = this.MarkPoints(this.Source, this.Source_points);
            _face_path = face_path;
            _face_points_path = face_path.Split('.')[0] + ".txt";
            _face = new ImageIO(face_path);
            _face.LockBits();
            _face_points = this.LoadPoints(this.Face_points_path);
            _face_marked = this.MarkPoints(this.Face, this.Face_points);
            _facePointsAffined = this.AffineTransformation();
            _TPS_transform_matrix = this.TPSTransformMatrix();
            _source.UnlockBits();
            _face.UnlockBits();
        }
        
        // 加载关键点信息
        private double[] LoadPoints(string pointsPath)
        {
            string[] pointsRaw = File.ReadAllLines(pointsPath, Encoding.UTF8);
            double[] points = new double[68 * 2];
            Parallel.For(0, 68, i =>
            {
                string[] pointArray = pointsRaw[i].Split(' ');
                points[2 * i] = Convert.ToDouble(pointArray[0], CultureInfo.InvariantCulture);
                points[2 * i + 1] = Convert.ToDouble(pointArray[1], CultureInfo.InvariantCulture);
            });
            return points;
        }

        // 标记关键点
        private ImageIO MarkPoints(ImageIO image, double[] points)
        {
            ImageIO marked = new ImageIO(image.Path, null);
            marked.LockBits();
            Color rgb = Color.FromArgb(43, 115, 175);
            Parallel.For(0, 68, i =>
              {
                  Parallel.For(0, 5, y_bias =>
                  {
                      Parallel.For(0, 5, x_bias =>
                      {
                          int y = Math.Max(0, Math.Min(image.Height - 1, Convert.ToInt32(points[2 * i + 1]) + y_bias - 2));
                          int x = Math.Max(0, Math.Min(image.Width - 1, Convert.ToInt32(points[2 * i]) + x_bias - 2));
                          marked.SetPixel(x, y, rgb);
                      });
                  });
              });
            marked.UnlockBits();
            //marked.SaveImage();
            return marked;
        }

        // * 仿射变换，目的对齐人脸，使得生成的图像较为自然，避免无用拉伸
        private double[] AffineTransformation()
        {
            double[][] sourcePointsMatrixData = new double[68][];
            double[][] facePointsMatrixData = new double[68][];
            double[] facePointsTransformed = new double[this.Face_points.Length];

            Parallel.For(0, 68, i =>
            {
                facePointsMatrixData[i] = new double[3];
                facePointsMatrixData[i][0] = this.Face_points[i * 2];
                facePointsMatrixData[i][1] = this.Face_points[i * 2 + 1];
                facePointsMatrixData[i][2] = 1;
                sourcePointsMatrixData[i] = new double[2];
                sourcePointsMatrixData[i][0] = this.Source_points[i * 2];
                sourcePointsMatrixData[i][1] = this.Source_points[i * 2 + 1];
            });
            MyMatrix sourcePointsMatrix = new MyMatrix(sourcePointsMatrixData);
            MyMatrix facePointsMatrix = new MyMatrix(facePointsMatrixData);
            MyMatrix affineTransformationMatrix = new MyMatrix(facePointsMatrix.Transpose().Multiply(facePointsMatrix).Inverse().Multiply(facePointsMatrix.Transpose().Multiply(sourcePointsMatrix)));
            // 并行计算
            Parallel.For(0, 68, i =>
            {
                facePointsTransformed[i * 2] = affineTransformationMatrix.GetMatrix_data()[0, 0] * this.Face_points[i * 2] + affineTransformationMatrix.GetMatrix_data()[1, 0] * this.Face_points[i * 2 + 1] + affineTransformationMatrix.GetMatrix_data()[2, 0];
                facePointsTransformed[i * 2 + 1] = affineTransformationMatrix.GetMatrix_data()[0, 1] * this.Face_points[i * 2] + affineTransformationMatrix.GetMatrix_data()[1, 1] * this.Face_points[i * 2 + 1] + affineTransformationMatrix.GetMatrix_data()[2, 1];
            });
            return facePointsTransformed;
        }

        // 径向基函数
        private static double RadialBasisFunction(double x1, double x2, double y1, double y2)
        {
            double r2 = Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2);
            if(r2!=0)
            {
                return r2 * Math.Log(r2);
            }
            else
            {
                return 0;
            }
        }

        // TPS变换矩阵求解
        private MyMatrix TPSTransformMatrix()
        {
            double[][] LMatrixData = new double[71][];
            double[][] YMatrixData = new double[71][];
            Parallel.For(0, 71, i =>
            {
                LMatrixData[i] = new double[71];
                Parallel.For(0, 71, j =>
                {
                    if ((i < 68) && (j < 68))
                    {
                        if (i == j)
                        {
                            LMatrixData[i][j] = 0;
                        }
                        else
                        {
                            LMatrixData[i][j] = RadialBasisFunction(this.FacePointsAffined[2 * i], this.FacePointsAffined[2 * j], this.FacePointsAffined[2 * i + 1], this.FacePointsAffined[2 * j + 1]);
                        }
                    }
                    else if ((i < 68) && (j >= 68))
                    {
                        switch (j)
                        {
                            case 68: LMatrixData[i][j] = 1; break;
                            case 69: LMatrixData[i][j] = this.FacePointsAffined[2 * i]; break;
                            case 70: LMatrixData[i][j] = this.FacePointsAffined[2 * i + 1]; break;
                        }
                    }
                    else if ((i >= 68) && (j < 68))
                    {
                        switch (i)
                        {
                            case 68: LMatrixData[i][j] = 1; break;
                            case 69: LMatrixData[i][j] = this.FacePointsAffined[2 * j]; break;
                            case 70: LMatrixData[i][j] = this.FacePointsAffined[2 * j + 1]; break;
                        }
                    }
                    else
                    {
                        LMatrixData[i][j] = 0;
                    }
                });
                YMatrixData[i] = new double[2];
                if (i < 68)
                {
                    YMatrixData[i][0] = this.Source_points[2 * i];
                    YMatrixData[i][1] = this.Source_points[2 * i + 1];
                }
                else
                {
                    YMatrixData[i][0] = 0;
                    YMatrixData[i][1] = 0;
                }
            });
            MyMatrix LMatrix = new MyMatrix(LMatrixData);
            MyMatrix YMatrix = new MyMatrix(YMatrixData);
            MyMatrix TransformMatrix = new MyMatrix(LMatrix.Inverse().Multiply(YMatrix).Transpose());
            return TransformMatrix;
        }

        // TPS变换，输入为新图像中的坐标，输出为原图像中的坐标
        public MyMatrix TPSProcessMatrix(double x,double y)
        {
            double[,] processMatrixData = new double[71, 1];
            Parallel.For(0, 71, i =>
            {
                if (i < 68)
                {
                    processMatrixData[i, 0] = RadialBasisFunction(this.FacePointsAffined[2 * i], x, this.FacePointsAffined[2 * i + 1], y);
                }
                else
                {
                    switch (i)
                    {
                        case 68:
                            {
                                processMatrixData[i, 0] = 1;
                                break;
                            }
                        case 69:
                            {
                                processMatrixData[i, 0] = x;
                                break;
                            }
                        case 70:
                            {
                                processMatrixData[i, 0] = y;
                                break;
                            }
                    }
                }
            });
            MyMatrix processMatrix = new MyMatrix(processMatrixData,71,1);
            MyMatrix process = new MyMatrix(processMatrix);
            return process;
        }
    }
}
