export PATH := /usr/local/bin:$(PATH)
XBUILD=msbuild
CONFIG=Release

KSPDIR=/Users/gwdp/Library/Application\ Support/Steam/steamapps/common/Kerbal\ Space\ Program
INSTALLDIR=$(KSPDIR)/GameData/KSPC-Driver
CONFIGDIR=$(INSTALLDIR)/PluginData/KSPC-Driver

PLUGINVERSION=$(shell egrep "^\[.*AssemblyVersion" KSPC-Driver/Properties/AssemblyInfo.cs|cut -d\" -f2)
PACKAGEDIR=package/KSPC-Driver
PACKAGECONFIGDIR=$(PACKAGEDIR)/PluginData/KSPC-Driver

all: dll

dll:
	$(XBUILD) /p:Configuration=$(CONFIG)

install:
	[ -d $(CONFIGDIR) ] || mkdir -p $(CONFIGDIR)
	cp KSPC-Driver/bin/$(CONFIG)/KSPC-Driver.dll $(INSTALLDIR)
	cp KSPC-Driver/bin/$(CONFIG)/PsimaxSerial.dll $(INSTALLDIR)
	cp config.xml $(CONFIGDIR)

clean:
	$(XBUILD) /p:Configuration=$(CONFIG) /t:Clean

package: all
	mkdir -p $(PACKAGECONFIGDIR)
	cp KSPC-Driver/bin/$(CONFIG)/KSPC-Driver.dll $(PACKAGEDIR)
	cp KSPC-Driver/bin/$(CONFIG)/PsimaxSerial.dll $(PACKAGEDIR)
	cp config.xml $(PACKAGECONFIGDIR)
	cd package; zip -r -9 ../KSPC-Driver-cross-$(PLUGINVERSION).zip KSPC-Driver
	rm -r package
	echo $(PLUGINVERSION) > KSPC-Driver.version

