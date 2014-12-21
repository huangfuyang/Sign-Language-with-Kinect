#include "opencv2/highgui/highgui.hpp"
#include "opencv2/imgproc/imgproc.hpp"
#include "opencv2/core/core.hpp"
#include "opencv2/ocl/ocl.hpp"
#include "opencv2/ml/ml.hpp"
#include "opencv2/legacy/legacy.hpp"
#include <iostream>
#include <stdio.h>
#include <stdlib.h>
#include <windows.h>
#include <fstream>
#include <math.h>
#include <ctime>

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

vector<string> get_all_files_names_within_folder(string folder)
{
	vector<string> names;
	char search_path[200];
	sprintf(search_path, "%s*.*", folder.c_str());
	WIN32_FIND_DATA fd; 
	HANDLE hFind = ::FindFirstFile(search_path, &fd); 
	if(hFind != INVALID_HANDLE_VALUE) 
	{ 
		do 
		{ 
			// read all (real) files in current folder
			// , delete '!' read other 2 default folder . and ..
			if(! (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) ) 
			{
				names.push_back(fd.cFileName);
			}
		}while(::FindNextFile(hFind, &fd)); 
		::FindClose(hFind); 
	} 
	return names;
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

fstream myfile;
fstream outfile;
void WriteData(vector<float> data)
{
	for (int i = 0; i < data.size(); i++)
	{
		myfile<<data[i]<<' ';
	}
	myfile<<endl;
}

void WriteData(Mat data,fstream& file)
{
	for (int i = 0; i < data.rows; i++)
	{
		for (int j = 0; j < data.cols; j++)
		{
			file<<data.at<double>(i,j)<<' ';
		}
		//file<<endl;
	}
}

Mat get_hogdescriptor_visual_image(Mat& origImg,
								   vector<float>& descriptorValues,
								   Size winSize,
								   Size cellSize,                                   
								   int scaleFactor,
								   double viz_factor)
{   
	Mat visual_image;
	resize(origImg, visual_image, Size(origImg.cols*scaleFactor, origImg.rows*scaleFactor));

	int gradientBinSize = 9;
	// dividing 180° into 9 bins, how large (in rad) is one bin?
	float radRangeForOneBin = 3.14/(float)gradientBinSize; 

	// prepare data structure: 9 orientation / gradient strenghts for each cell
	int cells_in_x_dir = winSize.width / cellSize.width;
	int cells_in_y_dir = winSize.height / cellSize.height;
	int totalnrofcells = cells_in_x_dir * cells_in_y_dir;
	float*** gradientStrengths = new float**[cells_in_y_dir];
	int** cellUpdateCounter   = new int*[cells_in_y_dir];
	for (int y=0; y<cells_in_y_dir; y++)
	{
		gradientStrengths[y] = new float*[cells_in_x_dir];
		cellUpdateCounter[y] = new int[cells_in_x_dir];
		for (int x=0; x<cells_in_x_dir; x++)
		{
			gradientStrengths[y][x] = new float[gradientBinSize];
			cellUpdateCounter[y][x] = 0;

			for (int bin=0; bin<gradientBinSize; bin++)
				gradientStrengths[y][x][bin] = 0.0;
		}
	}

	// nr of blocks = nr of cells - 1
	// since there is a new block on each cell (overlapping blocks!) but the last one
	int blocks_in_x_dir = cells_in_x_dir - 1;
	int blocks_in_y_dir = cells_in_y_dir - 1;

	// compute gradient strengths per cell
	int descriptorDataIdx = 0;
	int cellx = 0;
	int celly = 0;

	for (int blockx=0; blockx<blocks_in_x_dir; blockx++)
	{
		for (int blocky=0; blocky<blocks_in_y_dir; blocky++)            
		{
			// 4 cells per block ...
			for (int cellNr=0; cellNr<4; cellNr++)
			{
				// compute corresponding cell nr
				int cellx = blockx;
				int celly = blocky;
				if (cellNr==1) celly++;
				if (cellNr==2) cellx++;
				if (cellNr==3)
				{
					cellx++;
					celly++;
				}

				for (int bin=0; bin<gradientBinSize; bin++)
				{
					float gradientStrength = descriptorValues[ descriptorDataIdx ];
					descriptorDataIdx++;

					gradientStrengths[celly][cellx][bin] += gradientStrength;

				} // for (all bins)


				// note: overlapping blocks lead to multiple updates of this sum!
				// we therefore keep track how often a cell was updated,
				// to compute average gradient strengths
				cellUpdateCounter[celly][cellx]++;

			} // for (all cells)


		} // for (all block x pos)
	} // for (all block y pos)


	// compute average gradient strengths
	for (int celly=0; celly<cells_in_y_dir; celly++)
	{
		for (int cellx=0; cellx<cells_in_x_dir; cellx++)
		{

			float NrUpdatesForThisCell = (float)cellUpdateCounter[celly][cellx];

			// compute average gradient strenghts for each gradient bin direction
			for (int bin=0; bin<gradientBinSize; bin++)
			{
				gradientStrengths[celly][cellx][bin] /= NrUpdatesForThisCell;
			}
		}
	}


	cout << "descriptorDataIdx = " << descriptorDataIdx << endl;

	// draw cells
	for (int celly=0; celly<cells_in_y_dir; celly++)
	{
		for (int cellx=0; cellx<cells_in_x_dir; cellx++)
		{
			int drawX = cellx * cellSize.width;
			int drawY = celly * cellSize.height;

			int mx = drawX + cellSize.width/2;
			int my = drawY + cellSize.height/2;

			rectangle(visual_image,
				Point(drawX*scaleFactor,drawY*scaleFactor),
				Point((drawX+cellSize.width)*scaleFactor,
				(drawY+cellSize.height)*scaleFactor),
				CV_RGB(100,100,100),
				1);

			// draw in each cell all 9 gradient strengths
			for (int bin=0; bin<gradientBinSize; bin++)
			{
				float currentGradStrength = gradientStrengths[celly][cellx][bin];

				// no line to draw?
				if (currentGradStrength==0)
					continue;

				float currRad = bin * radRangeForOneBin + radRangeForOneBin/2;

				float dirVecX = cos( currRad );
				float dirVecY = sin( currRad );
				float maxVecLen = cellSize.width/2;
				float scale = viz_factor; // just a visual_imagealization scale,
				// to see the lines better

				// compute line coordinates
				float x1 = mx - dirVecX * currentGradStrength * maxVecLen * scale;
				float y1 = my - dirVecY * currentGradStrength * maxVecLen * scale;
				float x2 = mx + dirVecX * currentGradStrength * maxVecLen * scale;
				float y2 = my + dirVecY * currentGradStrength * maxVecLen * scale;

				// draw gradient visual_imagealization
				line(visual_image,
					Point(x1*scaleFactor,y1*scaleFactor),
					Point(x2*scaleFactor,y2*scaleFactor),
					CV_RGB(255,255,0),
					1);

			} // for (all bins)

		} // for (cellx)
	} // for (celly)


	// don't forget to free memory allocated by helper data structures!
	for (int y=0; y<cells_in_y_dir; y++)
	{
		for (int x=0; x<cells_in_x_dir; x++)
		{
			delete[] gradientStrengths[y][x];            
		}
		delete[] gradientStrengths[y];
		delete[] cellUpdateCounter[y];
	}
	delete[] gradientStrengths;
	delete[] cellUpdateCounter;

	return visual_image;

}



/** @function main */
int main( int argc, char** argv )
{

	//************ Load source image and convert it to gray****************
	string prepath = "C:\\Users\\Administrator\\Desktop\\handshapes\\standart hands\\out_resized5\\kmeantemplate\\";
	string prepath5 = "C:\\Users\\Administrator\\Desktop\\handshapes\\standart hands\\out_resized5\\";
	string suffixpath = ".jpg";
	myfile.open (prepath+"hog_60template15mean.txt");
	//outfile.open (prepath+"hog_templateKmean.txt",'w');
	// //computer k means
	//Mat labels;
	//int cluster_number = 20;
	//Mat centers;
	//cv::Mat input(60,4356,CV_32F);
	//std::vector<std::string> elems;
	//
	string line;
	int i=0;

	//while ( getline (myfile,line,' ') )
	//{
	//	input.at<float>(i/4356,i%4356) = atof(line.c_str());
	//	i++;
	//	if (i%4356 == 0)
	//	{
	//		cout<<i/4356<<endl;
	//	}
	//}
	//kmeans(input, cluster_number, labels, TermCriteria(CV_TERMCRIT_ITER|CV_TERMCRIT_EPS, 100, 0.0001), 1, cv::KMEANS_RANDOM_CENTERS, centers);
	//WriteData(centers,outfile);
	//outfile.close();
	//myfile.close();
	//******************** MOG********************
	//const int N = 6;
	//const int N1 = (int)sqrt((double)N);
	//
	////Mat labels;
	//const Scalar colors[] =
	//{
	//	Scalar(255,0,0), Scalar(0,255,0),
	//	Scalar(0,0,255),Scalar(255,255,0),
	//	Scalar(255,0,255),Scalar(0,255,255)
	//};
	//outfile.open("J:\\Kinect data\\mog141-180.txt",'w');
	//string path = "J:\\Kinect data\\Aaron 141-180\\hands\\";

	//vector<string> files = get_all_files_names_within_folder(path);
	//int size = files.size();
	//CvEM em_model;
	//CvEMParams params;
	//params.covs      = NULL;
	//params.means     = NULL;
	//params.weights   = NULL;
	//params.probs     = NULL;
	//params.nclusters = N;
	//params.cov_mat_type       = CvEM::COV_MAT_DIAGONAL;
	//params.start_step         = CvEM::START_AUTO_STEP;
	//params.term_crit.max_iter = 100;
	//params.term_crit.epsilon  = 1;
	//params.term_crit.type     = CV_TERMCRIT_ITER|CV_TERMCRIT_EPS;
	//for (int i = 0; i < size; i++)
	//{
	//	//clock_t begin = clock();
	//	vector<Point2f> vec;
	//	cout<<i<<'\\'<<size<<' '<<(float)i*100/size<<'%'<<endl;
	//	Mat img = imread(path+files[i]);

	//	for (int y = 0; y < img.rows; y++)
	//	{
	//		for (int x = 0; x  < img.cols; x ++)
	//		{
	//			if (img.ptr<Vec3b>(y)[x][0] != 0)
	//			{
	//				Point2f p(x,y);
	//				//samples.push_back(p);
	//				vec.push_back(p);
	//			}
	//		}
	//	}
	//	Mat samples(vec);

	//	samples = samples.reshape(1, 0);
	//	//cout<<samples<<endl;
	//	em_model.train( samples, Mat(), params);
	//	Mat means = em_model.getMeans();
	//	vector<Mat> covs ;
	//	em_model.getCovs(covs);
	//	outfile<<files[i].substr(0,files[i].length()-4)<<' ';
	//	WriteData(means,outfile);
	//	for (int i = 0; i < N; i++)
	//	{
	//		outfile<<covs[i].ptr<double>(0)[0]<<' '<<covs[i].ptr<double>(1)[1]<<' ';
	//	}
	//	outfile<<endl;

	//	//cout<<"**************"<<endl;
	//}
	//outfile.close();


	//draw the clustered samples
	/*for(int i = 0; i < nsamples; i++ )
	{
	Point pt(cvRound(samples.at<float>(i, 0)), cvRound(samples.at<float>(i, 1)));
	circle( img, pt, 1, colors[labels.at<int>(i)], CV_FILLED );
	}*/

	//imshow( "EM-clustering result", img );
	waitKey(0);
	//cout<<samples<<endl;
	//************* visualization******************
	const int length = 15;
	vector<vector<float>> hog(length);
	while ( getline (myfile,line,' ') )
	{
		if (i%4356 == 0)
		{
			hog[i/4356] = vector<float>(4356);
			cout<<i/4356<<endl;
		}
		float v = atof(line.c_str());
		hog[i/4356].at(i%4356) = v;
		i++;
		
		
	}

	Mat hogimg[length];
	for (int i = 0; i < length; i++)
	{
		hogimg[i] = Mat(60,60,CV_8UC3, Scalar(0,0,0));
		hogimg[i] = get_hogdescriptor_visual_image(hogimg[i],hog[i],Size(60,60),Size(5,5),5,5);
		imwrite(prepath5+"\\template\\"+to_string(i)+suffixpath,hogimg[i]);
	
	}



	//for (int i = 0; i < 60; i++)
	//{
	//	for (int j = 1; j < 6; j++)
	//	{
	//		string path =  prepath5 + to_string(i+1) +'_'+to_string(j)+suffixpath;
	//		Mat image = imread(path,1);
	//		vector<float> hog = Hog(image);
	//		WriteData(hog);
	//	}
	//	/*	for (int j = 1; j < 6; j++)
	//	{
	//	Mat rott;
	//	rotate(image,rott,j*45-135);
	//	rott = ResizeSign(rott);
	//	imwrite(prepath5+ to_string(i+1)+"_"+to_string(j)+suffixpath,rott);
	//	}*/

	//	cout<<i<<endl;
	//}


	//myfile <<to_string(d*100)<<" ";

	//char* source_window = "Source";
	//namedWindow( source_window, CV_WINDOW_NORMAL );

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

	//float s_in = 0,s_out = 0;

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