﻿using RetireSimple.Engine.Data;
using RetireSimple.Engine.Data.Investment;
using RetireSimple.Engine.Data.InvestmentVehicle;

namespace RetireSimple.Engine.Api {
	public class InvestmentApi {

		private readonly EngineDbContext _context;

		public InvestmentApi(EngineDbContext context) {
			_context = context;
		}

		/// <summary>
		/// Gets a list of <see cref="InvestmentBase"/> objects that are not 
		/// associated with an <see cref="Data.InvestmentVehicle.InvestmentVehicleBase"/>.
		/// </summary>
		/// <returns></returns>
		public List<InvestmentBase> GetAllInvestments() => throw new NotImplementedException();

		/// <summary>
		/// Adds a new investment of the specified type, with investment-specific parameters set 
		/// to the respective keys in <paramref name="parameters"/>. The analysis used can also be 
		/// set by adding the key-value pair <code>{["AnalysisType"] = //<<analysisNameString>></some></code>
		/// in <paramref name="parameters"/>.<br/>
		/// If <paramref name="parameters"/> does not contain a key for an investment-specifc parameter
		/// or for the type of analysis to use, the default value defined by the investment for that
		/// parameter is used. <br/>
		/// If <paramref name="parameters"/> is empty or null, the analysis used and other 
		/// investment-specific parameters will be set to their default values. 
		/// Throws an <see cref="ArgumentException"/> if <paramref name="type"/> does not match a
		/// known investment type.<br/>
		/// Do not use this method to set Analysis Parameters, use <see cref="UpdateAnalysisOption(int, string, string?)"/>
		/// for that functionality.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		/// <returns>ID of the newly created investment after it is saved in the database</returns>
		/// <exception cref="ArgumentException">The specified investment type was not found</exception>
		public int Add(string type, OptionsDict? parameters = null) {
			var newInvestment = new StockInvestment("");
			newInvestment.InvestmentName = parameters.GetValueOrDefault("investmentName");
			newInvestment.InvestmentType = parameters.GetValueOrDefault(type);
			newInvestment.StockPrice = Decimal.Parse(parameters.GetValueOrDefault("stockPrice"));
			newInvestment.StockQuantity = Decimal.Parse(parameters.GetValueOrDefault("stockQuantity"));
			newInvestment.StockTicker = parameters.GetValueOrDefault("stockTicker");
			newInvestment.StockPurchaseDate = DateTime.Parse(parameters.GetValueOrDefault("stockPurchaseDate"));
			newInvestment.StockDividendDistributionInterval = parameters.GetValueOrDefault("stockDividendDistributionInterval");
			newInvestment.StockDividendDistributionMethod = parameters.GetValueOrDefault("stockDividendDistributionMethod");
			newInvestment.StockDividendFirstPaymentDate = DateTime.Parse(parameters.GetValueOrDefault("stockDividendFirstPaymentDate"));

			var mainPortfolio = _context.Portfolio.First(p => p.PortfolioId == 1);
			mainPortfolio.Investments.Add(newInvestment);

			_context.SaveChanges();

			return _context.Investment.First(i => i.Equals(newInvestment)).InvestmentId;
		}

		//TODO implement this and for other investments
		private StockInvestment CreateStock(string name, OptionsDict? parameters) {
			return new StockInvestment("") {

			};
		}

		/// <summary>
		/// Removes the investment with the specified ID. If the specified investment has any 
		/// <see cref="InvestmentModel"/> or <see cref="Backend.DomainModel.Data.Expense.ExpeseBase"/>
		/// associated with it, those are deleted as well. <strong>However</strong>, if there are any
		/// <see cref="InvestmentTransfer"/> objects related to this investment in any form, an 
		/// <see cref="InvalidOperationException"/> is thrown. 
		/// </summary>
		/// <param name="id"></param>
		/// <exception cref="ArgumentException">If the specified investment does not exist</exception>
		/// <exception cref="InvalidOperationException">If the remove occurs while the investment still has
		/// some <see cref="InvestmentTransfer"/> objects related to it.</exception>
		public void Remove(int id) {
			throw new InvalidOperationException();
			throw new ArgumentException();
			{
				if(_context.InvestmentTransfer.First(it => it.SourceInvestmentId == id) != null
					|| _context.InvestmentTransfer.First(it => it.DestinationInvestmentId == id) == null) {
					throw new InvalidOperationException("Investment Transfer has objects related to deleted investment");
				} else if(_context.Investment.First(i => i.InvestmentId == id) == null) {
					throw new ArgumentException("ID does not exist");
				} else {
					_context.Remove(_context.Investment.First(i => i.InvestmentId == id));
					_context.SaveChanges();
					_context.Remove(_context.InvestmentModel.All(im => im.InvestmentId == id));
					_context.SaveChanges();
					_context.Remove(_context.Expense.All(e => e.SourceInvestmentId == id));
					_context.SaveChanges();
				}

			}

		}
		/// <summary>
		/// Updates the specified parameter in that specific investment. For simplicity, this method
		/// does not validate if the specified investment has such a parameter, rather it is inserted/overwritten 
		/// directly to the internal <see cref="InvestmentBase.InvestmentData"/> with <paramref name="param"/>
		/// as a key and <paramref name="value"/> as the value. Values cannot be null, and keys will not be
		/// removable from this dictionary.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="param"></param>
		/// <param name="value"></param>
		/// <exception cref="ArgumentException">If the specified investment does not exist</exception>
		public void Update(int id, string param, string value) {
			throw new NotImplementedException();
			{
				if(_context.Investment.First(i => i.InvestmentId == id) == null) {
					throw new NotImplementedException("Investment not found");
				}

				switch(param) {
					case "investmentName":
						_context.Investment.First(i => i.InvestmentId == id).InvestmentName = value;
						_context.SaveChanges();
					case "investmentType":
						_context.Investment.First(i => i.InvestmentId == id).InvestmentType = value;
						_context.SaveChanges();
					case "analysisType":
						_context.Investment.First(i => i.InvestmentId == id).AnalysisType = value;
						_context.SaveChanges();
				}
			}	
		}

		/// <summary>
		/// Updates many parameters in a specific investment based on the values defined in <paramref name="parameters"/>
		/// For simplicity, this method does not validate if the specified investment has such a parameter, 
		/// rather it is inserted/overwritten directly to the internal <see cref="InvestmentBase.InvestmentData"/>
		/// Values cannot be null, and keys will not be removable from this dictionary.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="parameters"></param>
		/// <exception cref="ArgumentException">If the specified investment does not exist</exception>
		public void Update(int id, OptionsDict parameters) {
			throw new NotImplementedException();
			{
				if(_context.Investment.First(i => i.InvestmentId == id) == null) {
					throw new NotImplementedException("Investment not found");
				}

				_context.Investment.First(i => i.InvestmentId == id).InvestmentData = parameters;
				_context.SaveChanges();
			}
		}

		/// <summary>
		/// Updates the <see cref="InvestmentBase.AnalysisOptionsOverrides"/> with the 
		/// specified analysis option and option value. No actual validation of the value with 
		/// respect to the analysis option is performed in this method. If <paramref name="value"/>
		/// is null and the option exists, it gets removed from the analysis overrides for the
		/// vehicle.
		/// </summary>
		/// <param name="vehicleId"></param>
		/// <param name="option"></param>
		/// <param name="value"></param>
		/// <exception cref="ArgumentException">If the specified option does not exist yet in the
		/// overrides and <paramref name="value"/> is null</exception>
		public void UpdateAnalysisOption(int id, string param, string? value) {
			throw new NotImplementedException();
			{
				if(_context.Investment.First(i => i.InvestmentId == id) == null) {
					throw new NotImplementedException("Investment not found");
				}

				if(_context.Investment.First(i => i.InvestmentId == id).AnalysisOptionsOverrides == null) {
				}
				//1.check parameter exists
				//2. value is null
				_context.SaveChanges();
			}
		}

		/// <summary>
		/// Returns a model of the investment based on that investment's chosen analysis and (optionally)
		/// any parameters that should be used as an override. If an existing <see cref="InvestmentModel"/>
		/// already exists and the investment's last change is before the time the model was generated, no analysis
		/// is executed and the existing model is returned. If <paramref name="options"/> is passed with a non-null
		/// value, the analysis is re-executed regardless if the model is up-to-date.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public InvestmentModel GetAnalysis(int id, OptionsDict? options = null) {
			throw new NotImplementedException();
			{
				if(_context.InvestmentModel.First(i => i.InvestmentModelId == id) == null) {
					throw new NotImplementedException("Investment not found");
				}

				if(options == null) {
					//check if investment model is null
					return _context.Investment.First(i => i.InvestmentId == id).InvestmentModel;
				} else {
					//invoke analysis
					_context.Investment.First(i => i.InvestmentId == id).InvokeAnalysis(options);
				}
			}
		}
	}
}
