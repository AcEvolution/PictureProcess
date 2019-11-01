using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureProcessing
{
    // 声明：ImageProcess 部分功能实现思路参考了互联网相关资料，具体实现为笔者自行完成，相关部分已用 * 号标注，未使用 * 标注为完全自主完成。
    class ImageProcess
    {
        // 变量存储区
        private ImageParameter _imageParameter;

        private ProcessOption _processOption;
        
        private ProcessResult _processResult;
        
        private DistortionParameter _distortionParameter;
        
        private WarpParameter _warpParameter;
        
        private InterpolationParameter _interpolationParameter;
        
        private FaceTransformation _faceTransformation;
        
        private readonly bool _stretch = true;

        // 数据接口
        internal ImageParameter ImageParameter => _imageParameter;

        internal ProcessOption ProcessOption => _processOption;

        internal ProcessResult ProcessResult => _processResult;

        internal DistortionParameter DistortionParameter => _distortionParameter;

        internal WarpParameter WarpParameter => _warpParameter;

        internal InterpolationParameter InterpolationParameter => _interpolationParameter;

        internal FaceTransformation FaceTransformation => _faceTransformation;

        public bool Stretch => _stretch;

        // 构造函数，根据参数不同判断所需进行的操作，转换类别存储在 ProcessOption 中
        public ImageProcess(ImageIO sourceImage,
            double maxDistortionDegree, double distortionRadius, int distortionCenter_x, int distortionCenter_y,
            int targetWidth, int targetHeight,
            Interpolation interpolation, double bicubicFactor = -0.5)
        {
            _imageParameter = new ImageParameter(sourceImage, null, sourceImage.Path, null);
            sourceImage.LockBits();
            _processOption = new ProcessOption(Transform.Distortion, interpolation);
            _distortionParameter = new DistortionParameter(maxDistortionDegree, distortionRadius, distortionCenter_x, distortionCenter_y);
            _warpParameter = null;
            _faceTransformation = null;
            _interpolationParameter = new InterpolationParameter(targetWidth, targetHeight, bicubicFactor);
            _processResult = new ProcessResult(this.InterpolationFunction(),null,null);
            sourceImage.UnlockBits();
        }

        public ImageProcess(ImageIO sourceImage,
            double warpFactor, int warpCenter_x, int warpCenter_y,
            int targetWidth, int targetHeight,
            Interpolation interpolation, double bicubicFactor = -0.5)
        {
            _stretch = false;
            _imageParameter = new ImageParameter(sourceImage, null, sourceImage.Path, null);
            sourceImage.LockBits();
            _processOption = new ProcessOption(Transform.Warp, interpolation);
            _distortionParameter = null;
            _warpParameter = new WarpParameter(sourceImage.Width * sourceImage.Height, warpFactor, warpCenter_x, warpCenter_y);
            _faceTransformation = null;
            _interpolationParameter = new InterpolationParameter(targetWidth, targetHeight, bicubicFactor);
            _processResult = new ProcessResult(this.InterpolationFunction(),null,null);
            sourceImage.UnlockBits();
        }

        public ImageProcess(ImageIO sourceImage,
            int targetWidth, int targetHeight,
            Interpolation interpolation, double bicubicFactor = -0.5)
        {
            _imageParameter = new ImageParameter(sourceImage, null, sourceImage.Path, null);
            sourceImage.LockBits();
            _processOption = new ProcessOption(Transform.None, interpolation);
            _distortionParameter = null;
            _warpParameter = null;
            _faceTransformation = null;
            _interpolationParameter = new InterpolationParameter(targetWidth, targetHeight, bicubicFactor);
            _processResult = new ProcessResult(this.InterpolationFunction(),null,null);
            sourceImage.UnlockBits();
        }

        public ImageProcess(ImageIO sourceImage, ImageIO faceImage,
            int targetWidth, int targetHeight,
            Interpolation interpolation, double bicubicFactor = -0.5)
        {
            _imageParameter = new ImageParameter(sourceImage, faceImage, sourceImage.Path, faceImage.Path);
            sourceImage.LockBits();
            faceImage.LockBits();
            _processOption = new ProcessOption(Transform.TPS, interpolation);
            _distortionParameter = null;
            _warpParameter = null;
            _faceTransformation = new FaceTransformation(this.ImageParameter.SourcePath, this.ImageParameter.FacePath);
            _interpolationParameter = new InterpolationParameter(targetWidth, targetHeight, bicubicFactor);
            _processResult = new ProcessResult(this.InterpolationFunction(),this.FaceTransformation.Source_marked,this.FaceTransformation.Face_marked);
            sourceImage.UnlockBits();
            faceImage.UnlockBits();
        }

        // 转换函数，输入为新图像位置，输出为原图像对应的位置
        private Poseition TransformFunction(double x, double y)
        {
            switch (this.ProcessOption.Transform)
            {
                // 旋转扭曲，使用题目给出的函数计算
                case Transform.Distortion:
                    {
                        if (this.DistortionParameter == null)
                        {
                            return null;
                        }
                        else
                        {
                            double new_x = x;
                            double new_y = y;
                            double distance = Math.Sqrt(Math.Pow(Convert.ToDouble(x - this.DistortionParameter.DistortionCenter_x) / this.DistortionParameter.Factor_x, 2) + Math.Pow(Convert.ToDouble(y - this.DistortionParameter.DistortionCenter_y) / this.DistortionParameter.Factor_y, 2));
                            if (distance < this.DistortionParameter.DistortionRadius)
                            {
                                double distortionDegree = -this.DistortionParameter.MaxDistortionDegree * ((this.DistortionParameter.DistortionRadius - distance) / this.DistortionParameter.DistortionRadius);
                                new_x = ((x - this.DistortionParameter.DistortionCenter_x) * Math.Cos(distortionDegree) - (y - this.DistortionParameter.DistortionCenter_y) * Math.Sin(distortionDegree)) + this.DistortionParameter.DistortionCenter_x;
                                new_y = ((x - this.DistortionParameter.DistortionCenter_x) * Math.Sin(distortionDegree) + (y - this.DistortionParameter.DistortionCenter_y) * Math.Cos(distortionDegree)) + this.DistortionParameter.DistortionCenter_y;
                            }
                            return new Poseition(new_x, new_y);
                        }

                    }
                // * 畸变矫正，题目给出的函数实现效果不佳，查询网络相关实现方法后，学习、改进并实现了自己的转换方法
                case Transform.Warp:
                    {
                        if (this.WarpParameter == null)
                        {
                            return null;
                        }
                        else
                        {
                            double new_x = x;
                            double new_y = y;
                            double distance = Math.Sqrt(Math.Pow(Convert.ToDouble(x - this.WarpParameter.WarpCenter_x) / this.WarpParameter.Factor_x, 2) + Math.Pow(Convert.ToDouble(y - this.WarpParameter.WarpCenter_y) / this.WarpParameter.Factor_y, 2));
                            if (distance != 0)
                            {
                                double k = this.WarpParameter.WarpFactor * (Math.Pow(distance, 2) / this.WarpParameter.WarpScale);
                                new_x = (1 + k) * (x - this.WarpParameter.WarpCenter_x) + this.WarpParameter.WarpCenter_x;
                                new_y = (1 + k) * (y - this.WarpParameter.WarpCenter_y) + this.WarpParameter.WarpCenter_y;
                            }
                            return new Poseition(new_x, new_y);
                        }
                    }
                // TPS变换，使用题目给的变换方法并做了相关改进，详见 FaceTransformation.cs
                case Transform.TPS:
                    {
                        MyMatrix newPosition = new MyMatrix(this.FaceTransformation.TPS_transform_matrix.Multiply(this.FaceTransformation.TPSProcessMatrix(x, y)));
                        return new Poseition(newPosition.GetMatrix_data()[0, 0], newPosition.GetMatrix_data()[1, 0]);
                    }
                // 无变换，直接插值
                case Transform.None:
                    {
                        return new Poseition(x, y);
                    }
                default: return null;
            }
        }
        
        // 插值函数，输入为原图像，输出为目标尺寸大小的图像，输出尺寸任意，由 ProcessOption 决定
        private ImageIO InterpolationFunction()
        {
            switch (this.ProcessOption.Interpolation)
            {
                case Interpolation.None:
                    {
                        return this.NoneInterpolationCore();
                    }
                case Interpolation.NearestNeighbor:
                    {
                        return this.NearestNeighborInterpolationCore();
                    }
                case Interpolation.BiLinear:
                    {
                        return this.BiIinearInterpolationCore();
                    }
                case Interpolation.BiCubic:
                    {
                        return this.BiCubicInterpolationCore();
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        // 无插值
        private ImageIO NoneInterpolationCore()
        {
            ImageIO processed = new ImageIO(null, this.InterpolationParameter.TargetWidth, this.InterpolationParameter.TargetHeight);
            processed.LockBits();
            //并行计算
            Info infoBox = new Info();
            infoBox.Show();
            Parallel.For(0, this.InterpolationParameter.TargetHeight, y =>
            {
                Parallel.For(0, this.InterpolationParameter.TargetWidth, x =>
                {
                    Poseition transformPoseition = this.TransformFunction(Convert.ToDouble(x), Convert.ToDouble(y));
                    if(transformPoseition.X<0 || transformPoseition.X>(this.ImageParameter.SourceImage.Width - 1)|| transformPoseition.Y < 0 || transformPoseition.Y > (this.ImageParameter.SourceImage.Height - 1))
                    {
                        if(this.Stretch)
                        {
                            Poseition newPoseition = new Poseition(Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Width - 1, transformPoseition.X)), Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Height - 1, transformPoseition.Y)));
                            System.Drawing.Color rgb = this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(newPoseition.X), Convert.ToInt32(newPoseition.Y));
                            processed.SetPixel(x, y, rgb);
                        }
                        else
                        {
                            System.Drawing.Color rgb = System.Drawing.Color.FromArgb(0, 0, 0);
                            processed.SetPixel(x, y, rgb);
                        }
                    }
                    else
                    {
                        Poseition newPoseition = new Poseition(Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Width - 1, transformPoseition.X)), Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Height - 1, transformPoseition.Y)));
                        System.Drawing.Color rgb = this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(newPoseition.X), Convert.ToInt32(newPoseition.Y));
                        processed.SetPixel(x, y, rgb);
                    }
                });
            });
            infoBox.Close();
            Success successBox = new Success();
            successBox.ShowDialog();
            processed.UnlockBits();
            return processed;
        }
        
        // 最近邻插值
        private ImageIO NearestNeighborInterpolationCore()
        {
            ImageIO processed = new ImageIO(null, this.InterpolationParameter.TargetWidth, this.InterpolationParameter.TargetHeight);
            processed.LockBits();
            double widthFactor = Convert.ToDouble(this.ImageParameter.SourceImage.Width - 1) / Convert.ToDouble(this.InterpolationParameter.TargetWidth - 1);
            double heightFactor = Convert.ToDouble(this.ImageParameter.SourceImage.Height - 1) / Convert.ToDouble(this.InterpolationParameter.TargetHeight - 1);
            //并行计算
            Info infoBox = new Info();
            infoBox.Show();
            Parallel.For(0, this.InterpolationParameter.TargetHeight, y =>
            {
                Parallel.For(0, this.InterpolationParameter.TargetWidth, x =>
                {
                    Poseition transformPoseition = this.TransformFunction(Convert.ToDouble(x) * widthFactor, Convert.ToDouble(y) * heightFactor);
                    Poseition interpolatePosition = new Poseition(transformPoseition.X * widthFactor, transformPoseition.Y * heightFactor);
                    if (transformPoseition.X < 0 || transformPoseition.X > (this.ImageParameter.SourceImage.Width - 1) || transformPoseition.Y < 0 || transformPoseition.Y > (this.ImageParameter.SourceImage.Height - 1))
                    {
                        if (this.Stretch)
                        {
                            Poseition newPoseition = new Poseition(Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Width - 1, transformPoseition.X)), Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Height - 1, transformPoseition.Y)));
                            System.Drawing.Color rgb = this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(newPoseition.X), Convert.ToInt32(newPoseition.Y));
                            processed.SetPixel(x, y, rgb);
                        }
                        else
                        {
                            System.Drawing.Color rgb = System.Drawing.Color.FromArgb(0, 0, 0);
                            processed.SetPixel(x, y, rgb);
                        }
                    }
                    else
                    {
                        Poseition newPoseition = new Poseition(Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Width - 1, transformPoseition.X)), Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Height - 1, transformPoseition.Y)));
                        System.Drawing.Color rgb = this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(newPoseition.X), Convert.ToInt32(newPoseition.Y));
                        processed.SetPixel(x, y, rgb);
                    }
                });
            });
            infoBox.Close();
            Success successBox = new Success();
            successBox.ShowDialog();
            processed.UnlockBits();
            return processed;
        }
        
        // 双线性插值
        private ImageIO BiIinearInterpolationCore()
        {
            ImageIO processed = new ImageIO(null, this.InterpolationParameter.TargetWidth, this.InterpolationParameter.TargetHeight);
            processed.LockBits();
            double widthFactor = Convert.ToDouble(this.ImageParameter.SourceImage.Width - 1) / Convert.ToDouble(this.InterpolationParameter.TargetWidth - 1);
            double heightFactor = Convert.ToDouble(this.ImageParameter.SourceImage.Height - 1) / Convert.ToDouble(this.InterpolationParameter.TargetHeight - 1);
            //并行计算
            Info infoBox = new Info();
            infoBox.Show();
            Parallel.For(0, this.InterpolationParameter.TargetHeight, y =>
            {
                Parallel.For(0, this.InterpolationParameter.TargetWidth, x =>
                {
                    Poseition transformPoseition = this.TransformFunction(Convert.ToDouble(x) * widthFactor, Convert.ToDouble(y) * heightFactor);
                    Poseition interpolatePosition = new Poseition(transformPoseition.X, transformPoseition.Y);
                    Poseition interpolatePositionFloor = new Poseition(Math.Floor(transformPoseition.X), Math.Floor(transformPoseition.Y));
                    Poseition interpolatePosition11 = new Poseition(Math.Max(0, Math.Min(this.InterpolationParameter.TargetWidth - 1, interpolatePositionFloor.X)), Math.Max(0, Math.Min(this.InterpolationParameter.TargetHeight - 1, interpolatePositionFloor.Y)));
                    Poseition interpolatePosition12 = new Poseition(Math.Max(0, Math.Min(this.InterpolationParameter.TargetWidth - 1, interpolatePositionFloor.X)), Math.Max(0, Math.Min(this.InterpolationParameter.TargetHeight - 1, interpolatePositionFloor.Y + 1)));
                    Poseition interpolatePosition21 = new Poseition(Math.Max(0, Math.Min(this.InterpolationParameter.TargetWidth - 1, interpolatePositionFloor.X + 1)), Math.Max(0, Math.Min(this.InterpolationParameter.TargetHeight - 1, interpolatePositionFloor.Y)));
                    Poseition interpolatePosition22 = new Poseition(Math.Max(0, Math.Min(this.InterpolationParameter.TargetWidth - 1, interpolatePositionFloor.X + 1)), Math.Max(0, Math.Min(this.InterpolationParameter.TargetHeight - 1, interpolatePositionFloor.Y + 1)));
                    if (transformPoseition.X <= 0 || transformPoseition.X >= (this.ImageParameter.SourceImage.Width - 1) || transformPoseition.Y <= 0 || transformPoseition.Y >= (this.ImageParameter.SourceImage.Height - 1))
                    {
                        if (this.Stretch)
                        {
                            Poseition newPoseition = new Poseition(Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Width - 1, transformPoseition.X)), Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Height - 1, transformPoseition.Y)));
                            System.Drawing.Color rgb = this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(newPoseition.X), Convert.ToInt32(newPoseition.Y));
                            processed.SetPixel(x, y, rgb);
                        }
                        else
                        {
                            System.Drawing.Color rgb = System.Drawing.Color.FromArgb(0, 0, 0);
                            processed.SetPixel(x, y, rgb);
                        }
                    }
                    else
                    {
                        double[] source_pixels_weight_sum = { 0, 0, 0 };
                        byte[] rgb = { 0, 0, 0 };
                        source_pixels_weight_sum[0] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition11.X), Convert.ToInt32(interpolatePosition11.Y)).R) * (interpolatePosition22.X - interpolatePosition.X) * (interpolatePosition22.Y - interpolatePosition.Y);
                        source_pixels_weight_sum[0] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition12.X), Convert.ToInt32(interpolatePosition12.Y)).R) * (interpolatePosition22.X - interpolatePosition.X) * (interpolatePosition.Y - interpolatePosition11.Y);
                        source_pixels_weight_sum[0] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition21.X), Convert.ToInt32(interpolatePosition21.Y)).R) * (interpolatePosition.X - interpolatePosition11.X) * (interpolatePosition22.Y - interpolatePosition.Y);
                        source_pixels_weight_sum[0] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition22.X), Convert.ToInt32(interpolatePosition22.Y)).R) * (interpolatePosition.X - interpolatePosition11.X) * (interpolatePosition.Y - interpolatePosition11.Y);

                        source_pixels_weight_sum[1] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition11.X), Convert.ToInt32(interpolatePosition11.Y)).G) * (interpolatePosition22.X - interpolatePosition.X) * (interpolatePosition22.Y - interpolatePosition.Y);
                        source_pixels_weight_sum[1] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition12.X), Convert.ToInt32(interpolatePosition12.Y)).G) * (interpolatePosition22.X - interpolatePosition.X) * (interpolatePosition.Y - interpolatePosition11.Y);
                        source_pixels_weight_sum[1] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition21.X), Convert.ToInt32(interpolatePosition21.Y)).G) * (interpolatePosition.X - interpolatePosition11.X) * (interpolatePosition22.Y - interpolatePosition.Y);
                        source_pixels_weight_sum[1] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition22.X), Convert.ToInt32(interpolatePosition22.Y)).G) * (interpolatePosition.X - interpolatePosition11.X) * (interpolatePosition.Y - interpolatePosition11.Y);

                        source_pixels_weight_sum[2] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition11.X), Convert.ToInt32(interpolatePosition11.Y)).B) * (interpolatePosition22.X - interpolatePosition.X) * (interpolatePosition22.Y - interpolatePosition.Y);
                        source_pixels_weight_sum[2] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition12.X), Convert.ToInt32(interpolatePosition12.Y)).B) * (interpolatePosition22.X - interpolatePosition.X) * (interpolatePosition.Y - interpolatePosition11.Y);
                        source_pixels_weight_sum[2] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition21.X), Convert.ToInt32(interpolatePosition21.Y)).B) * (interpolatePosition.X - interpolatePosition11.X) * (interpolatePosition22.Y - interpolatePosition.Y);
                        source_pixels_weight_sum[2] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(interpolatePosition22.X), Convert.ToInt32(interpolatePosition22.Y)).B) * (interpolatePosition.X - interpolatePosition11.X) * (interpolatePosition.Y - interpolatePosition11.Y);
                        for (int i = 0; i < 3; i++)
                        {
                            rgb[i] = Convert.ToByte(Math.Max(0, Math.Min(255, source_pixels_weight_sum[i])));
                        }
                        System.Drawing.Color rgb_color = System.Drawing.Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                        processed.SetPixel(x, y, rgb_color);
                    }
                });
            });
            infoBox.Close();
            Success successBox = new Success();
            successBox.ShowDialog();
            processed.UnlockBits();
            return processed;
        }
        
        // 双三次插值权重函数
        static private double BiCubicWeight(double x, double bicubicFactor)
        {
            if (Math.Abs(x) <= 1)
            {
                return (bicubicFactor + 2) * Math.Pow(Math.Abs(x), 3) - (bicubicFactor + 3) * Math.Pow(Math.Abs(x), 2) + 1;
            }
            else if (Math.Abs(x) < 2)
            {
                return bicubicFactor * Math.Pow(Math.Abs(x), 3) - 5 * bicubicFactor * Math.Pow(Math.Abs(x), 2) + 8 * bicubicFactor * Math.Pow(Math.Abs(x), 1) - 4 * bicubicFactor;
            }
            else
            {
                return 0;
            }
        }

        // 双三次插值
        private ImageIO BiCubicInterpolationCore()
        {
            ImageIO processed = new ImageIO(null, this.InterpolationParameter.TargetWidth, this.InterpolationParameter.TargetHeight);
            processed.LockBits();
            double widthFactor = Convert.ToDouble(this.ImageParameter.SourceImage.Width - 1) / Convert.ToDouble(this.InterpolationParameter.TargetWidth - 1);
            double heightFactor = Convert.ToDouble(this.ImageParameter.SourceImage.Height - 1) / Convert.ToDouble(this.InterpolationParameter.TargetHeight - 1);
            //并行计算
            Info infoBox = new Info();
            infoBox.Show();
            Parallel.For(0, this.InterpolationParameter.TargetHeight, y =>
            {
                Parallel.For(0, this.InterpolationParameter.TargetWidth, x =>
                {
                    Poseition transformPoseition = this.TransformFunction(Convert.ToDouble(x) * widthFactor, Convert.ToDouble(y) * heightFactor);
                    Poseition interpolatePosition = new Poseition(transformPoseition.X - 0.5, transformPoseition.Y - 0.5);
                    if (transformPoseition.X < 0 || transformPoseition.X > (this.ImageParameter.SourceImage.Width - 1) || transformPoseition.Y < 0 || transformPoseition.Y > (this.ImageParameter.SourceImage.Height - 1))
                    {
                        if (this.Stretch)
                        {
                            double[] source_pixels_weight_sum = { 0, 0, 0 };
                            byte[] rgb = { 0, 0, 0 };
                            Parallel.For(0, 4, y_index =>
                            {
                                Parallel.For(0, 4, x_index =>
                                {
                                    Poseition dPosition = new Poseition(-interpolatePosition.X + Convert.ToInt32(interpolatePosition.X) + x_index - 1, -interpolatePosition.Y + Convert.ToInt32(interpolatePosition.Y) + y_index - 1);
                                    Poseition positionWeight = new Poseition(BiCubicWeight(dPosition.X, this.InterpolationParameter.BicubicFactor), BiCubicWeight(dPosition.Y, this.InterpolationParameter.BicubicFactor));
                                    Poseition indexPosition = new Poseition(Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Width - 1, Convert.ToInt32(interpolatePosition.X) + x_index - 1)), Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Height - 1, Convert.ToInt32(interpolatePosition.Y) + y_index - 1)));
                                    source_pixels_weight_sum[0] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(indexPosition.X), Convert.ToInt32(indexPosition.Y)).R) * positionWeight.X * positionWeight.Y;
                                    source_pixels_weight_sum[1] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(indexPosition.X), Convert.ToInt32(indexPosition.Y)).G) * positionWeight.X * positionWeight.Y;
                                    source_pixels_weight_sum[2] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(indexPosition.X), Convert.ToInt32(indexPosition.Y)).B) * positionWeight.X * positionWeight.Y;
                                });
                            });
                            for (int i = 0; i < 3; i++)
                            {
                                rgb[i] = Convert.ToByte(Math.Max(0, Math.Min(255, source_pixels_weight_sum[i])));
                            }
                            System.Drawing.Color rgb_color = System.Drawing.Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                            processed.SetPixel(x, y, rgb_color);
                        }
                        else
                        {
                            System.Drawing.Color rgb = System.Drawing.Color.FromArgb(0, 0, 0);
                            processed.SetPixel(x, y, rgb);
                        }
                    }
                    else
                    {
                        double[] source_pixels_weight_sum = { 0, 0, 0 };
                        byte[] rgb = { 0, 0, 0 };
                        Parallel.For(0, 4, y_index =>
                        {
                            Parallel.For(0, 4, x_index =>
                            {
                                Poseition dPosition = new Poseition(-interpolatePosition.X + Convert.ToInt32(interpolatePosition.X) + x_index - 1, -interpolatePosition.Y + Convert.ToInt32(interpolatePosition.Y) + y_index - 1);
                                Poseition positionWeight = new Poseition(BiCubicWeight(dPosition.X, this.InterpolationParameter.BicubicFactor), BiCubicWeight(dPosition.Y, this.InterpolationParameter.BicubicFactor));
                                Poseition indexPosition = new Poseition(Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Width - 1, Convert.ToInt32(interpolatePosition.X) + x_index - 1)), Math.Max(0, Math.Min(this.ImageParameter.SourceImage.Height - 1, Convert.ToInt32(interpolatePosition.Y) + y_index - 1)));
                                source_pixels_weight_sum[0] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(indexPosition.X), Convert.ToInt32(indexPosition.Y)).R) * positionWeight.X * positionWeight.Y;
                                source_pixels_weight_sum[1] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(indexPosition.X), Convert.ToInt32(indexPosition.Y)).G) * positionWeight.X * positionWeight.Y;
                                source_pixels_weight_sum[2] += Convert.ToDouble(this.ImageParameter.SourceImage.GetPixel(Convert.ToInt32(indexPosition.X), Convert.ToInt32(indexPosition.Y)).B) * positionWeight.X * positionWeight.Y;
                            });
                        });
                        for (int i = 0; i < 3; i++)
                        {
                            rgb[i] = Convert.ToByte(Math.Max(0, Math.Min(255, source_pixels_weight_sum[i])));
                        }
                        System.Drawing.Color rgb_color = System.Drawing.Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                        processed.SetPixel(x, y, rgb_color);
                    }
                });
            });
            infoBox.Close();
            Success successBox = new Success();
            successBox.ShowDialog();
            processed.UnlockBits();
            return processed;
        }
    }
}
