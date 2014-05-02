#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/ocl/ocl.hpp"
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <windows.h>

using namespace cv;
using namespace std;

Mat image; Mat src_gray;
int thresh = 200;
int max_thresh = 255;
RNG rng(12345);
Mat images[4][4];
/// Function header
void thresh_callback(int, void* );

vector<float> Hog(Mat image)
{
	Mat threshold_output;
	cvtColor( image, src_gray, CV_BGR2GRAY );
	vector<float> descriptors;
	//find hand
	threshold( src_gray, threshold_output, thresh, 255, THRESH_BINARY_INV );
	vector<vector<Point> > contours;
	vector<Vec4i> hierarchy;
	Mat copyBinaryImg;
	threshold_output.copyTo(copyBinaryImg);
	findContours( threshold_output, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0) );
	Rect r;
	for( int i = 0; i < contours.size(); i++ )
	{ 
		if( contours[i].size() > 5 )
		{
			r = boundingRect( Mat(contours[i]));
		}
	}
	Mat s_image(copyBinaryImg,r);

	Mat dst_img_rsize(60,60,s_image.type());
	resize(s_image,dst_img_rsize,dst_img_rsize.size(),0,0,INTER_LINEAR);
	//imshow("Source",s_image);
	//imshow("Source",threshold_output);
	//resize(threshold_output,dst_img_rsize,dst_img_rsize.size(),0,0,INTER_LINEAR);
	HOGDescriptor* hog = new HOGDescriptor(cvSize(60, 60), cvSize(10, 10), cvSize(5, 5), cvSize(5, 5), 9);
	hog->compute(dst_img_rsize,descriptors, Size(1, 1), Size(0, 0));
	return descriptors;
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
	return sum;
}
/** @function main */
int main( int argc, char** argv )
{
	/// Load source image and convert it to gray
	string prepath = "C:\\Users\\Administrator\\Desktop\\handshapes\\handshape";
	string suffixpath = ".jpg";
	vector<float> hogs[4][4];
	for (int i = 0; i < 4; i++)
	{
		for (int j = 0; j < 4; j++)
		{
			string path =  prepath + to_string(i+1) +"-"+to_string(j+1)+suffixpath;
			images[i][j] = imread(path,IMREAD_COLOR);
			hogs[i][j] = Hog(images[i][j]);
		}
	}
	
	//blur( src_gray, src_gray, Size(3,3) );

	/// Create Window
	char* source_window = "Source";
	//namedWindow( source_window, CV_WINDOW_NORMAL );
	//  imshow( source_window, image );

	createTrackbar( " Threshold:", "Source", &thresh, max_thresh, thresh_callback );
	//thresh_callback( 0, 0 );
	
	for (int j = 0; j < 16; j++)
	{
		for (int i = 0; i < 16; i++)
		{
			float d = GetDistance(hogs[j/4][j%4],hogs[i/4][i%4]);
		
			printf("%.1f ",d*100);
			if (i%4 == 3)
			{
				printf(" ");
			}
		}
		printf("\n");
		if (j%4 == 3)
		{
		printf("\n");

		}
	}
	for (int j = 0; j < 16; j++)
	{
		for (int i = 0; i < 16; i++)
		{
			float d = GetDistance(hogs[j/4][j%4],hogs[i/4][i%4]);
			int t = d< 0.022?1:0;
			printf("%d ",t);
			if (i%4 == 3)
			{
				printf(" ");
			}
		}
		printf("\n");
		if (j%4 == 3)
		{
		printf("\n");

		}
	}	
		printf("\n");


	system("pause");
	return(0);
}


/** @function thresh_callback */
void thresh_callback(int, void* )
{
	Mat threshold_output;
	vector<vector<Point> > contours;
	vector<Vec4i> hierarchy;

	/// Detect edges using Threshold
	threshold( src_gray, threshold_output, thresh, 255, THRESH_BINARY_INV );
	/// Find contours
	findContours( threshold_output, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0) );

	/// Find the rotated rectangles and ellipses for each contour
	vector<Rect> minRect( contours.size() );
	vector<RotatedRect> minEllipse( contours.size() );

	for( int i = 0; i < contours.size(); i++ )
	{ 
		minRect[i] = boundingRect( Mat(contours[i]) );
		if( contours[i].size() > 5 )
		{ minEllipse[i] = fitEllipse( Mat(contours[i]) ); }
	}

	/// Draw contours + rotated rects + ellipses
	Mat drawing = Mat::zeros( threshold_output.size(), CV_8UC3 );
	for( int i = 0; i< contours.size(); i++ )
	{
		Scalar color = Scalar( rng.uniform(0, 255), rng.uniform(0,255), rng.uniform(0,255) );
		// contour
		drawContours( drawing, contours, i, color, 1, 8, vector<Vec4i>(), 0, Point() );
		// ellipse
		ellipse( drawing, minEllipse[i], color, 2, 8 );
		//// rotated rectangle
		//Point2f rect_points[4]; minRect[i].points( rect_points );
		//for( int j = 0; j < 4; j++ )
		//   line( drawing, rect_points[j], rect_points[(j+1)%4], color, 1, 8 );
		rectangle(drawing,minRect[i],color);
	}

	/// Show in a window
	namedWindow( "Contours", CV_WINDOW_NORMAL );
	imshow( "Contours", threshold_output );
}