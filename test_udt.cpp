#include <arpa/inet.h>
#include <udt.h>
#include <stdio.h>
#include <string.h>

int main(int argc,char *argv[])
{
	if(argc<4)
	{
		fprintf(stderr,"wrong arguments!\n");
		return -1;
	}
	UDTSOCKET skt=UDT::socket(AF_INET,SOCK_STREAM,0);
	sockaddr_in serv_addr={0},cli_addr={0};
	if(argc>4)
	{
		cli_addr.sin_family=AF_INET;
		cli_addr.sin_port=htons(atoi(argv[4]));
		cli_addr.sin_addr=htonl(INADDR_ANY);
		if(UDT::bind(skt,(sockaddr *)&cli_addr,sizeof(cli_addr))==UDT::ERROR)
		{
			fprintf(stderr,"binding port failed:%s\n",UDT::getlasterror().getErrorMessage());
			UDT::close(skt);
			return -1;
		}
	}
	serv_addr.sin_family=AF_INET;
	serv_addr.sin_port=htons(atoi(argv[2]));
	inet_pton(AF_INET,argv[1],&serv_addr.sin_addr);
	if(UDT::ERROR==UDT::connect(skt,(sockaddr*)&serv_addr,sizeof(serv_addr)))
	{
		fprintf(stderr,"connect wrong:%s",UDT::getlasterror().getErrorMessage());
		UDT::close(skt);
		return -1;
	}
	printf("send message:%s\n",argv[3]);
	UDT::send(skt,argv[3],strlen(argv[3]),0);
	UDT::close(skt);
	printf("done!\n");
	return 0;
}
