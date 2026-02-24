using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using Services;
using System.Threading.Tasks;
using Moq;
using RepositoryContracts;
using AutoFixture;
using FluentAssertions;

namespace Tests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;
    private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
    private readonly ICountriesRepository _countriesRepository;
    private readonly IFixture _fixture;

    public CountriesServiceTest()
    {
        _fixture = new Fixture();

        _countriesRepositoryMock = new();
        _countriesRepository = _countriesRepositoryMock.Object;
        _countriesService = new CountriesService(_countriesRepository);
    }

    #region AddCountry

    // When CountryAddRequest is null, it should throw ArgumentNullException
    [Fact]
    public async Task AddCountry_NullCountry_ToBeArgumentNullException()
    {
        // Arrange
        CountryAddRequest? request = null;

        Country country = _fixture.Build<Country>()
            .With(temp => temp.Persons, null as List<Person>)
            .Create();

        _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
            .ReturnsAsync(country);

        // Act
        var action = async () =>
        {
            await _countriesService.AddCountry(request);
        };

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When the CountryName is null, it should throw ArgumentException
    [Fact]
    public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
    {
        // Arrange
        CountryAddRequest? request = _fixture.Build<CountryAddRequest>()
            .With(temp => temp.CountryName, null as string)
            .Create();

        Country country = _fixture.Build<Country>()
            .With(temp => temp.Persons, null as List<Person>)
            .Create();

        _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
            .ReturnsAsync(country);

        // Act
        var action = async () =>
        {
            await _countriesService.AddCountry(request);
        };

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When the CountryName is duplicate, it should throw ArgumentException
    [Fact]
    public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
    {
        // Arrange
        CountryAddRequest? request1 = _fixture.Build<CountryAddRequest>()
            .With(temp => temp.CountryName, "Test name")
            .Create();
        CountryAddRequest? request2 = _fixture.Build<CountryAddRequest>()
            .With(temp => temp.CountryName, "Test name")
            .Create();

        Country country1 = request1.ToCountry();
        Country country2 = request2.ToCountry();

        _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
            .ReturnsAsync(country1);
        _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
            .ReturnsAsync(null as Country);

        CountryResponse country1_from_add_country = await _countriesService.AddCountry(request1);

        // Act 
        var action = async () =>
        {
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country1);
            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync(country1);

            await _countriesService.AddCountry(request2);
        };

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When you supply proper country name, it should insert (add) the country to the existing list of countries
    [Fact]
    public async Task AddCountry_FullCountry_ToBeSuccessful()
    {
        // Arrange
        CountryAddRequest? request = _fixture.Create<CountryAddRequest>();
        Country country = request.ToCountry();
        CountryResponse country_response = country.ToCountryResponse();

        _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>()))
            .ReturnsAsync(country);
        _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
            .ReturnsAsync(null as Country);

        // Act
        CountryResponse response = await _countriesService.AddCountry(request);

        country.CountryID = response.CountryID;
        country_response.CountryID = response.CountryID;

        // Assert
        response.CountryID.Should().NotBe(Guid.Empty);
        response.Should().BeEquivalentTo(country_response);
    }

    #endregion

    #region GetAllCountries

    [Fact]
    // The list of countries should be empty by default (before adding any countries)
    public async Task GetAllCountries_EmptyList()
    {
        // Arrange
        List<Country> country_empty_list = new();
        _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
            .ReturnsAsync(country_empty_list);

        // Act
        List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();

        // Assert
        actual_country_response_list.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllCountries_AddFewCountries()
    {
        // Arrange
        List<Country> country_list = new()
        {
            _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create(),
            _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create()
        };

        List<CountryResponse> country_response_list = country_list.Select(temp => temp.ToCountryResponse()).ToList();

        _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
            .ReturnsAsync(country_list);

        // Act
        List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

        // Assert
        actualCountryResponseList.Should().BeEquivalentTo(country_response_list);
    }

    #endregion

    #region GetCountryByCountryID

    [Fact]
    // If we supply null as CountryID, it should return null as CountryResponse
    public async Task GetCountryByCountryID_NullCountryID_ToBeNull()
    {
        // Arrange
        Guid? countryID = null;

        _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
            .ReturnsAsync(null as Country);

        // Act
        CountryResponse? country_response_from_get_method = await _countriesService.GetCountryByCountryID(countryID);

        // Assert
        country_response_from_get_method.Should().BeNull(); 
    }

    [Fact]
    // If we supply a valid country id, it should return the matching country details as CountryResponse object
    public async Task GetCountryByCountryID_ValidCountryID_ToBeSuccessful()
    {
        // Arrange
        Country country = _fixture.Build<Country>()
            .With(temp => temp.Persons, null as List<Person>)
            .Create();
        CountryResponse country_response = country.ToCountryResponse();

        _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
            .ReturnsAsync(country);

        // Act
        CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryID(country.CountryID);

        // Assert
        country_response_from_get.Should().Be(country_response);
    }

    #endregion
}
