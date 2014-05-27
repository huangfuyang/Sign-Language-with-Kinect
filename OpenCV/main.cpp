#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/ocl/ocl.hpp"
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <windows.h>
#include <fstream>
#include <math.h>


using namespace cv;
using namespace std;

Mat image; Mat src_gray;
int thresh = 50;
int max_thresh = 255;
RNG rng(12345);
Mat images[60];
/// Function header
void thresh_callback(int, void* );

vector<float> Hog(Mat image)
{
	vector<float> descriptors;
	HOGDescriptor* hog = new HOGDescriptor(cvSize(60, 60), cvSize(10, 10), cvSize(5, 5), cvSize(5, 5), 9);
	hog->compute(image,descriptors, Size(1, 1), Size(0, 0));
	return descriptors;
}

void rotate(const Mat& src, Mat& dst, float angle)
{
	CV_Assert(!src.empty());

	float radian = angle /180.0 * 3.14159;

	int uniSize = max(src.cols, src.rows) * 2;
	int dx = (uniSize - src.cols) / 2;
	int dy = (uniSize - src.rows) / 2;

	copyMakeBorder(src, dst, dy, dy, dx, dx, BORDER_CONSTANT);

	//旋轉中心
	Point2f center(dst.cols/2, dst.rows/2);
	Mat affine_matrix = getRotationMatrix2D( center, angle, 1.0 );

	warpAffine(dst, dst, affine_matrix, dst.size());

	float sinVal = fabs(sin(radian));
	float cosVal = fabs(cos(radian));

	//旋轉后的圖像大小
	Size targetSize(src.cols * cosVal + src.rows * sinVal,
		src.cols * sinVal + src.rows * cosVal);

	//剪掉四周边框
	int x = (dst.cols - targetSize.width) / 2;
	int y = (dst.rows - targetSize.height) / 2;

	Rect rect(x, y, targetSize.width, targetSize.height);
	dst = Mat(dst, rect);
}

Mat ResizeSign(Mat image)
{
	Mat threshold_output;
	Mat gray;
	cvtColor(image,gray,CV_BGR2GRAY);

	//find hand
	threshold( gray, threshold_output, thresh, 255, THRESH_BINARY );
	vector<vector<Point> > contours;
	vector<Vec4i> hierarchy;
	Mat copyBinaryImg;
	threshold_output.copyTo(copyBinaryImg);
	Mat drawing = Mat::zeros( threshold_output.size(), CV_8UC3 );
	findContours( threshold_output, contours, hierarchy, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE, Point(0, 0) );
	Rect r;
	Scalar color = cvScalarAll(255);

	for( int i = 0; i < contours.size(); i++ )
	{ 
		if( contours[i].size() > 50 )
		{
			drawContours( drawing, contours, i, color, -1, 8, vector<Vec4i>(), 0, Point() );
			r = boundingRect( Mat(contours[i]));
		}
	}

	if (r.width>r.height)
	{
		r.y -= (r.width-r.height)/2;
		r.height = r.width;
	}else
	{
		r.x -= (r.height-r.width)/2;
		r.width = r.height;
	}
	int l = r.width>r.height?r.width:r.height;
	r.x = r.x>0?r.x:0;
	r.y = r.y>0?r.y:0;
	r.width = (r.width+r.x)>copyBinaryImg.cols?copyBinaryImg.cols-r.x:r.width;
	r.height = (r.height+r.y)>copyBinaryImg.rows?copyBinaryImg.rows-r.y:r.height;
	Mat s_image(copyBinaryImg,r);

	Mat dst_img_rsize(60,60,s_image.type());
	resize(s_image,dst_img_rsize,dst_img_rsize.size(),0,0,INTER_LINEAR);
	return dst_img_rsize;
}

float GetDistance(vector<float> v1, vector<float> v2)
{
	float sum = 0;
	for (int i = 0; i < v1.size(); i++)
	{
		float diff = v1[i]-v2[i];
		sum+=diff*diff;

	}
	sum/=v1.size();
	return sqrt(sum);
}

ofstream myfile;
void WriteData(vector<float> data)
{
	for (int i = 0; i < data.size(); i++)
	{
		myfile<<data[i]<<' ';
	}
	myfile<<endl;
}


/** @function main */
int main( int argc, char** argv )
{
	/// Load source image and convert it to gray
	string prepath = "C:\\Users\\Administrator\\Desktop\\handshapes\\standart hands\\out_resized8\\";
	string prepath5 = "C:\\Users\\Administrator\\Desktop\\handshapes\\standart hands\\out_resized5\\";
	string suffixpath = ".jpg";
	myfile.open ("hog_template5.txt");
	for (int i = 0; i < 60; i++)
	{
		for (int j = 1; j < 6; j++)
		{
			string path =  prepath5 + to_string(i+1) +'_'+to_string(j)+suffixpath;
			Mat image = imread(path,1);
			vector<float> hog = Hog(image);
			WriteData(hog);
		}
	/*	for (int j = 1; j < 6; j++)
		{
			Mat rott;
			rotate(image,rott,j*45-135);
			rott = ResizeSign(rott);
			imwrite(prepath5+ to_string(i+1)+"_"+to_string(j)+suffixpath,rott);
		}*/

		cout<<i<<endl;
	}


	//myfile <<to_string(d*100)<<" ";

	myfile.close();
	char* source_window = "Source";
	namedWindow( source_window, CV_WINDOW_NORMAL );
	/*Mat rott;
	rotate(images[0],rott,90);
	imshow(source_window,rott);*/
	//image = imread(prepath + "1" +suffixpath,1);
	//cvtColor(image,src_gray,CV_BGR2GRAY);

	/*if (image.data)
	{
	imshow( source_window, src_gray );
	}*/

	//createTrackbar( " Threshold:", "Source", &thresh, max_thresh, thresh_callback );
	//thresh_callback( 0, 0 );

	float s_in = 0,s_out = 0;

	//for (int j = 0; j < 16; j++)
	//{
	//	for (int i = 0; i < 16; i++)
	//	{

	//		float d = GetDistance(hogs[j/4][j%4],hogs[i/4][i%4]);
	//		if (j/4 == i/4)
	//		{
	//			s_in+=d;
	//		}
	//		else
	//		{
	//			s_out+=d;
	//		}
	//		printf("%.1f ",d*256);
	//		if (i%4 == 3)
	//		{
	//			printf(" ");
	//		}
	//	}
	//	printf("\n");
	//	if (j%4 == 3)
	//	{
	//		printf("\n");

	//	}
	//}
	//printf("in:%.2f  out:%.2f \n",s_in*100/64,s_out*100/192);

	waitKey();
	system("pause");
	return(0);
}


/** @function thresh_callback */
void thresh_callback(int, void* )
{
	Mat threshold_output;
	threshold( src_gray, threshold_output, thresh, 255, THRESH_BINARY );
	vector<vector<Point>> contours;
	vector<Vec4i> hierarchy;
	findContours( threshold_output, contours, hierarchy, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE, Point(0, 0) );
	Rect r;
	Scalar color = cvScalarAll(255);


	/// Find the rotated rectangles and ellipses for each contour
	vector<Rect> minRect( contours.size() );
	vector<RotatedRect> minEllipse( contours.size() );

	for( int i = 0; i < contours.size(); i++ )
	{ 
		minRect[i] = boundingRect( Mat(contours[i]) );
		if( contours[i].size() > 50 )
		{ minEllipse[i] = fitEllipse( Mat(contours[i]) ); }
	}

	/// Draw contours + rotated rects + ellipses
	Mat drawing = Mat::zeros( threshold_output.size(), CV_8UC3 );
	for( int i = 0; i< contours.size(); i++ )
	{
		// contour
		drawContours( drawing, contours, i, color, -1, 8, vector<Vec4i>(), 0, Point() );
		//cv::Mat mask;
		//cv::Point seed(30,30);
		//cv::Canny(drawing, mask, 100, 200);
		//cv::copyMakeBorder(mask, mask, 1, 1, 1, 1, cv::BORDER_REPLICATE);
		////Fill mask with value 128
		//uchar fillValue = 128;
		//cv::floodFill(drawing, mask, seed, cv::Scalar(255) ,0, cv::Scalar(), cv::Scalar(), 4 | cv::FLOODFILL_MASK_ONLY | (fillValue << 8));

		// ellipse
		//ellipse( drawing, minEllipse[i], color, 2, 8 );
		//// rotated rectangle
		//Point2f rect_points[4]; minRect[i].points( rect_points );
		//for( int j = 0; j < 4; j++ )
		//   line( drawing, rect_points[j], rect_points[(j+1)%4], color, 1, 8 );
		//rectangle(drawing,minRect[i],color);
	}

	//vertical
	for (int i = 0; i < 100; i++)
	{
		bool flag = false;
		for (int j = 0; j < 100; j++)
		{
			byte cell = threshold_output.data[i*100+j];


		}
	}
	/// Show in a window
	namedWindow( "thresh", CV_WINDOW_NORMAL );
	imshow( "thresh", threshold_output*255 );
	namedWindow( "Contours", CV_WINDOW_NORMAL );
	imshow( "Contours", drawing );
}