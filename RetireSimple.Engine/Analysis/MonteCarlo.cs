using MathNet.Numerics.Distributions;

using RetireSimple.Engine.Data.Analysis;
using RetireSimple.Engine.Data.Investment;

using System.Collections.Concurrent;

namespace RetireSimple.Engine.Analysis {

	public enum MonteCarloRV {
		NORMAL,
		LOGNORMAL,
		//The following are currently not implemented, but are supported by Math.NET
		CONTINUOUSUNIF,
		BETA,
		CAUCHY,
		CHI,
		CHISQ,
		ERLANG,
		EXPONENTIAL,
		FISHERSNEDECOR,
		GAMMA,
		GAMMAINV,
		LAPLACE,
		PARETO,
		RAYLEIGH,
		STABLE,
		STUDENTT,
		WEIBULL,
		TRIANGULAR
	}

	public class MonteCarlo {

		/// <summary>
		/// Options Data Structure for Monte Carlo Simulation Parameters
		/// </summary>
		public readonly record struct MonteCarloOptions {
			public decimal BasePrice { get; init; }
			public int AnalysisLength { get; init; }
			public decimal RandomVarScaleFactor { get; init; }
			public IContinuousDistribution RandomVariable { get; init; }
		}

		/// <summary>
		/// Utility Function to generate a Math.NET Continuous Distribution for use in Monte Carlo.
		/// Parameters for the distribution should be added to the parameters dictionary with the key
		/// for the respective variable. At the moment, the following keys are considered valid for
		/// setting up distributions:
		/// <br/>
		/// - "Mu" - Mu parameter (used in Normal, LogNormal, and Student T distributions)<br/>
		/// - "Sigma" - Sigma Parameter (used in Normal, LogNormal, and Student T distributions)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		internal static IContinuousDistribution CreateRandomVarInstance(MonteCarloRV type, Dictionary<string, double> parameters) {
			return type switch {
				MonteCarloRV.NORMAL => new Normal(parameters["Mu"], parameters["Sigma"]),
				MonteCarloRV.LOGNORMAL => new LogNormal(parameters["Mu"], parameters["Sigma"]),
				_ => throw new NotImplementedException(),
			};
		}

		public static List<decimal> MonteCarloSimSingleIteration(MonteCarloOptions options) {
			var currentPrice = options.BasePrice;
			var iterModel = new List<decimal>();

			for (var step = 0; step < options.AnalysisLength; step++) {
				iterModel.Add(currentPrice);
				currentPrice += options.RandomVarScaleFactor
								* (decimal)options.RandomVariable.Sample();
			}

			return iterModel;
		}


		/// <summary>
		/// Monte Carlo Simulation using a "scaled" normal random variable to simulate the random walk. <br/>
		/// This is used to only simulate the random walk of the stock's price over time, post-processing is
		/// required for getting actual value of the stock
		/// <br/>
		/// Used Analysis Options: <br/>
		/// - "AnalysisLength": Number of Months to Simulate Analysis Over<br/>
		/// - "SimCount": Number of Simulations to perform <br/>
		/// - "RandomVariableMu": The Expectation (mu) of the Normal Distribution <br/>
		/// - "RandomVariableSigma": The Standard Deviation (sigma) of the Normal Distribution <br/>
		/// - "RandomVarialbeScaleFactor": The "scaling factor" to apply
		/// to random variable samples. This is parsed as a <see cref="decimal"/>.
		/// </summary>
		/// <param name="stock"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static InvestmentModel MonteCarloSimNormal(StockInvestment stock, OptionsDict options) {
			//Extract Required Data for simulation purposes
			var rvOptions = new Dictionary<string, double>() {
				["Mu"] = double.Parse(options["RandomVariableMu"]),
				["Sigma"] = double.Parse(options["RandomVariableSigma"])
			};
			var simOptions = new MonteCarloOptions() {
				BasePrice = stock.StockPrice,
				AnalysisLength = int.Parse(options["AnalysisLength"]),
				RandomVarScaleFactor = decimal.Parse(options["RandomVariableScaleFactor"]),
				RandomVariable = CreateRandomVarInstance(MonteCarloRV.NORMAL, rvOptions)
			};
			var maxIterations = int.Parse(options["SimCount"]);

			//Threading the task because .NET concurrency pretty sick
			var simLists = new ConcurrentBag<List<decimal>>();
			Parallel.For(0, maxIterations, x => {
				simLists.Add(MonteCarloSimSingleIteration(simOptions));
			});

			return FilterSimulationData(simLists, simOptions.AnalysisLength);
		}

		/// <summary>
		/// Monte Carlo Simulation using a "scaled" lognormal random variable to simulate the random walk. <br/>
		/// This is used to only simulate the random walk of the stock's price over time, post-processing is
		/// required for getting actual value of the stock
		/// <br/>
		/// Used Analysis Options: <br/>
		/// - "AnalysisLength": Number of Months to Simulate Analysis Over<br/>
		/// - "SimCount": Number of Simulations to perform <br/>
		/// - "RandomVariableMu": The Expectation (mu) of the Normal Distribution <br/>
		/// - "RandomVariableSigma": The Standard Deviation (sigma) of the Normal Distribution <br/>
		/// - "RandomVarialbeScaleFactor": The "scaling factor" to apply
		/// to random variable samples. This is parsed as a <see cref="decimal"/>.
		/// </summary>
		/// <param name="stock"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static InvestmentModel MonteCarloSimLogNormal(StockInvestment stock, OptionsDict options) {
			//Extract Required Data for simulation purposes
			var rvOptions = new Dictionary<string, double>() {
				["Mu"] = double.Parse(options["RandomVariableMu"]),
				["Sigma"] = double.Parse(options["RandomVariableSigma"])
			};
			var simOptions = new MonteCarloOptions() {
				BasePrice = stock.StockPrice,
				AnalysisLength = int.Parse(options["AnalysisLength"]),
				RandomVarScaleFactor = decimal.Parse(options["RandomVariableScaleFactor"]),
				RandomVariable = CreateRandomVarInstance(MonteCarloRV.LOGNORMAL, rvOptions)
			};
			var maxIterations = int.Parse(options["SimCount"]);

			//Threading the task because .NET concurrency pretty sick
			var simLists = new ConcurrentBag<List<decimal>>();
			Parallel.For(0, maxIterations, x => {
				simLists.Add(MonteCarloSimSingleIteration(simOptions));
			});

			return FilterSimulationData(simLists, simOptions.AnalysisLength);
		}

		public static InvestmentModel FilterSimulationData(ConcurrentBag<List<decimal>> simLists, int analysisLength) {
			var model = new InvestmentModel();
			for (int i = 0; i < analysisLength; i++) {
				model.MinModelData.Add(simLists.Select(x => x[i]).Min());
				model.MaxModelData.Add(simLists.Select(x => x[i]).Max());
				model.AvgModelData.Add(simLists.Select(x => x[i]).Average());
			}

			return model;
		}

	}
}
