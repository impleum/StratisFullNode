using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class ImplxMain : Network
    {
        public ImplxMain()
        {
            this.Name = "ImplxMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("ImpX"));
            this.DefaultPort = 18105;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 18104;
            this.DefaultAPIPort = 18103;
            this.DefaultSignalRPort = 18102;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = Money.Coins(0.1m);
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = ImplxNetwork.ImplxRootFolderName;
            this.DefaultConfigFilename = ImplxNetwork.ImplxDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "IMPLX";
            this.DefaultBanTimeSeconds = 11250; // 500 (MaxReorg) * 45 (TargetSpacing) / 2 = 3 hours, 7 minutes and 30 seconds

            this.CirrusRewardDummyAddress = "CPqxvnzfXngDi75xBJKqi4e6YrFsinrJka"; // Cirrus main address
            this.RewardClaimerBatchActivationHeight = 119_200; // Tuesday, 12 January 2021 9:00:00 AM (Estimated)
            this.RewardClaimerBlockInterval = 100;

            // To successfully process the OP_FEDERATION opcode the federations should be known.
            this.Federations = new Federations();
            this.Federations.RegisterFederation(new Federation(new[]
            {
                new PubKey("03797a2047f84ba7dcdd2816d4feba45ae70a59b3aa97f46f7877df61aa9f06a21"),
                new PubKey("0209cfca2490dec022f097114090c919e85047de0790c1c97451e0f50c2199a957"),
                new PubKey("032e4088451c5a7952fb6a862cdad27ea18b2e12bccb718f13c9fdcc1caf0535b4"),
                new PubKey("035bf78614171397b080c5b375dbb7a5ed2a4e6fb43a69083267c880f66de5a4f9"),
                new PubKey("02387a219b1de54d4dc73a710a2315d957fc37ab04052a6e225c89205b90a881cd"),
                new PubKey("028078c0613033e5b4d4745300ede15d87ed339e379daadc6481d87abcb78732fa"),
                new PubKey("02b3e16d2e4bbad6dba1e699934a52d58d9b60b6e7eed303e400e95f2dbc2ef3fd"),
                new PubKey("02ba8b842997ce50c8e29c24a5452de5482f1584ae79778950b7bae24d4cc68dad"),
                new PubKey("02cbd907b0bf4d757dee7ea4c28e63e46af19dc8df0c924ee5570d9457be2f4c73"),
                new PubKey("02d371f3a0cffffcf5636e6d4b79d9f018a1a18fbf64c39542b382c622b19af9de"),
                new PubKey("02f891910d28fc26f272da8d7f548fdc18c286704907673e839dc07e8df416c15e"),
                new PubKey("0337e816a3433c71c4bbc095a54a0715a6da7a70526d2afb8dba3d8d78d33053bf"),
                new PubKey("035569e42835e25c854daa7de77c20f1009119a5667494664a46b5154db7ee768a"),
                new PubKey("03cda7ea577e8fbe5d45b851910ec4a795e5cc12d498cf80d39ba1d9a455942188"),
                new PubKey("02680321118bce869933b07ea42cc04d2a2804134b06db582427d6b9688b3536a4")}));

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1607706917; // ~11 December 2020 - https://www.unixtimestamp.com/
            this.GenesisNonce = 1752306; // Set to 1 until correct value found
            this.GenesisBits = 0x1E0FFFFF; // The difficulty target
            this.GenesisVersion = 1; // 'Empty' BIP9 deployments as they are all activated from genesis already
            this.GenesisReward = Money.Zero;

            Block genesisBlock = ImplxNetwork.CreateGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward, "Bloomberg 11/30/2020 Bitcoin Is Winning the Covid-19 Monetary Revolution");

            this.Genesis = genesisBlock;

            // Taken from Stratis.
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5,
                witnessScaleFactor: 4
            );

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new ImplxBIP9Deployments()
            {
                // Always active.
                [ImplxBIP9Deployments.CSV] = new BIP9DeploymentsParameters("CSV", 0, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [ImplxBIP9Deployments.Segwit] = new BIP9DeploymentsParameters("Segwit", 1, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold),
                [ImplxBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2, BIP9DeploymentsParameters.AlwaysActive, 999999999, BIP9DeploymentsParameters.DefaultMainnetThreshold)
            };

            this.Consensus = new NBitcoin.Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 770, // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: null,
                minerConfirmationWindow: 2016,
                maxReorgLength: 500,
                defaultAssumeValid: null, // TODO: Set this once some checkpoint candidates have elapsed
                maxMoney: Money.Coins(21000000), //long.MaxValue,
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(8734567),
                proofOfWorkReward: Money.Coins(1),
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60),
                targetSpacing: TimeSpan.FromSeconds(45),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 675,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(1)
            );

            this.Consensus.PosEmptyCoinbase = false;

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { 104 }; // X
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { 129 }; // y
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (104 + 129) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x000002e1a5c2361c43f5d76b7d77cd52c2866b391c59867ad79de49795ed7361"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) },
                 { 675, new CheckpointInfo(new uint256("0x228fe79897e110963fb1c39fc87f8627b51b37151d2184178139960b9ddc902a"), new uint256("0xb6191b4b733d38c0d3a7d1bcf95009bc8b173fc9dd435165bf08cde2eef33918")) },
                 { 1000, new CheckpointInfo(new uint256("0x6637657c16eb9ce7b99d09f41c0e12054dc0c47fd566f69af7778770bd5107f9"), new uint256("0x00eb64d91bb3dade131e3baf57fd40120586c0b08eea1852a2268b4eb4d87787")) },
                 { 1500, new CheckpointInfo(new uint256("0x21215e41cfa79a83b32c34419c9ff24712d5be48e2753ea58c80422432198f93"), new uint256("0x23e3130f9b7d4101a129c03792ac565f0850a6df475ac59bae77ae4761c6dc98")) },
                 { 2000, new CheckpointInfo(new uint256("0x6ae90bfcb5d1b6f1da5144f5f6e496fe9ab7e799f6fabffdeb4a4d30e50bb023"), new uint256("0x246398d97a66f88ac66319d38814d4a5369b7735aa595ad11f00e40ea4622d9a")) },
                 { 2500, new CheckpointInfo(new uint256("0x6857ebfe3ee8fdad274a6ae9baa3eb92f0fa5e2b99015eb9db526e52a4ea93ee"), new uint256("0xe8712f66a3d334ad4fd2b4d5a533de98d7acc6df8065ba55e46812e5c7066f4d")) },
                 { 5000, new CheckpointInfo(new uint256("0x1c246ba592df8017204016ca22ec07cde07750108e2ddabaa9e42235e6e24b2c"), new uint256("0x18b5001c53a2df968c266a243ea31cefa9b90681a64a7f11aea0cf2637261215")) },
                 { 10000, new CheckpointInfo(new uint256("0xf031eb6405232137e6fe1a44537c64f44d0991d6c9e42b16bab5b284029b1d7e"), new uint256("0x8a66e739b226a6752cd193aab96095084368bb3c36195bf1a7d8a41f81109e8f")) },
                 { 20000, new CheckpointInfo(new uint256("0xb24fca13c138617da441b4a983e582557cda12fb778d7fae0994af01e3f3d252"), new uint256("0x35c3f5856f36fdc43ee7da31530e649966b924cf59e1fddd87d7ed7837b63073")) },
                 { 30000, new CheckpointInfo(new uint256("0x8e7f225cb9eed57d578a6edf98a61f4120d20f0b41032eef35b557c2ae3b1c5e"), new uint256("0xd204fc2c31a15f5e3c02a178d100d88d1c10d21087c7a321594590f471ea6e51")) },
                 { 50000, new CheckpointInfo(new uint256("0x6d0541ad988fb15b9b59a291f0beca1cd147dcc60e7503d4610f21aedeee4053"), new uint256("0x9fdfb977228155cb14c814e463f6280ece1961e873165dd9eadabbc523ee35fe")) },
                 { 70000, new CheckpointInfo(new uint256("0x3f3d906896235395d6e203a1967875ba9408cc56d9a49b75503ef507712a7642"), new uint256("0x989fe82bec93afd796121443e62dab9a20fdb81764f5b52fceff05b05b4a781a")) }
            };

            this.Bech32Encoders = new Bech32Encoder[2];
            var encoder = new Bech32Encoder("implx");
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("seed1", "mn1.uh420058.ukrdomen.com"),
                new DNSSeedData("seed2", "mn2.uh420058.ukrdomen.com"),
                new DNSSeedData("seed3", "mn3.uh420058.ukrdomen.com"),
                new DNSSeedData("seed4", "mn4.uh420058.ukrdomen.com")
            };

            this.SeedNodes = new List<NetworkAddress>
            {
            };

            this.StandardScriptsRegistry = new ImplxStandardScriptsRegistry();

            Assert(this.DefaultBanTimeSeconds <= this.Consensus.MaxReorgLength * 64 / 2); //this.Consensus.MaxReorgLength * this.Consensus.TargetSpacing.TotalSeconds / 2);
            
            // TODO: Update these when the final block is mined
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x000155d1942054636a5a5ad37e9c7aa79bb0a430dea7017e1d3ed9e1be535e45"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x63ef070e33a794ea56ce116970b7779714364e4641d27ce78d0bb868f5ffca2d"));

            ImplxNetwork.RegisterRules(this.Consensus);
            ImplxNetwork.RegisterMempoolRules(this.Consensus);
        }
    }
}
