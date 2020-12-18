#include <stdio.h>
#include <vector>
#include <arpa/inet.h>

#include "udt.h"

int main(int argc,char *argv[])
{
	int nPort = 1234;
	int len;
	UDTSOCKET skt;
	std::pair<UDTSOCKET, sockaddr_in> peer1,peer2;
	std::vector<std::pair<UDTSOCKET,sockaddr_in>> aClients;
	if (argc > 1)
	{
		nPort = atoi(argv[1]);
	}
	UDTSOCKET sktServer = UDT::socket(AF_INET, SOCK_STREAM, 0);
	sockaddr_in serAddr = { 0 }, cliAddr = { 0 };
	serAddr.sin_family = AF_INET;
	serAddr.sin_port = htons(nPort);
	serAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	if(UDT::bind(sktServer,(sockaddr *)&serAddr,sizeof(serAddr))==UDT::ERROR)
	{
		fprintf(stderr, "binding port failed:%s\n", UDT::getlasterror().getErrorMessage());
		UDT::close(sktServer);
		return -1;
	}
	if (UDT::listen(sktServer, 10) == UDT::ERROR)
	{
		fprintf(stderr, "listening failed:%s\n", UDT::getlasterror().getErrorMessage());
		UDT::close(sktServer);
		return -1;
	}
	printf("Start listening...\n");
	while (true)
	{
		len = sizeof(cliAddr);
		skt = UDT::accept(sktServer, (sockaddr *)& cliAddr, &len);
		peer1.first = skt;
		peer1.second = cliAddr;
		aClients.push_back(peer1);
		if (aClients.size() % 2)
			continue;
		peer2 = aClients[aClients.size() - 2];
		UDT::send(peer1.first, (char*)& peer2.second, sizeof(peer2.second), 0);
		UDT::send(peer2.first, (char*)& peer1.second, sizeof(peer1.second), 0);
	}
    return 0;
}