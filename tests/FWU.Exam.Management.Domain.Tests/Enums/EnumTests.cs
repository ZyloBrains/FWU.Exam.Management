using FWU.Exam.Management.Domain.Enums;
using FluentAssertions;

namespace FWU.Exam.Management.Domain.Tests.Enums;

public class EnumTests
{
    [Fact]
    public void ApplicationStatus_ShouldHaveExpectedValues()
    {
        ((int)ApplicationStatus.Submitted).Should().Be(1);
        ((int)ApplicationStatus.UnderReview).Should().Be(2);
        ((int)ApplicationStatus.Approved).Should().Be(3);
        ((int)ApplicationStatus.Rejected).Should().Be(4);
    }

    [Fact]
    public void RegistrationStatus_ShouldHaveExpectedValues()
    {
        ((int)RegistrationStatus.Pending).Should().Be(1);
        ((int)RegistrationStatus.CollegeVerified).Should().Be(2);
        ((int)RegistrationStatus.AdminVerified).Should().Be(3);
        ((int)RegistrationStatus.Registered).Should().Be(4);
        ((int)RegistrationStatus.Withheld).Should().Be(5);
        ((int)RegistrationStatus.Rejected).Should().Be(6);
    }

    [Fact]
    public void TenantType_ShouldHaveExpectedValues()
    {
        ((int)TenantType.Standard).Should().Be(0);
        ((int)TenantType.Central).Should().Be(1);
    }

    [Fact]
    public void EnrollmentType_ShouldHaveExpectedValues()
    {
        ((int)EnrollmentType.FullTime).Should().Be(1);
        ((int)EnrollmentType.PartTime).Should().Be(2);
        ((int)EnrollmentType.Audit).Should().Be(3);
        ((int)EnrollmentType.Repeating).Should().Be(4);
    }

    [Fact]
    public void ResultStatus_ShouldHaveExpectedValues()
    {
        ((int)ResultStatus.Passed).Should().Be(0);
        ((int)ResultStatus.Failed).Should().Be(1);
        ((int)ResultStatus.Incomplete).Should().Be(2);
        ((int)ResultStatus.Withdrawn).Should().Be(3);
        ((int)ResultStatus.Absent).Should().Be(4);
    }

    [Fact]
    public void PaymentStatus_ShouldHaveExpectedValues()
    {
        ((int)PaymentStatus.Pending).Should().Be(0);
        ((int)PaymentStatus.Paid).Should().Be(1);
        ((int)PaymentStatus.Failed).Should().Be(2);
        ((int)PaymentStatus.Cancelled).Should().Be(3);
        ((int)PaymentStatus.Refunded).Should().Be(4);
    }

    [Fact]
    public void CollegeStatus_ShouldHaveExpectedValues()
    {
        ((int)CollegeStatus.Active).Should().Be(1);
        ((int)CollegeStatus.Inactive).Should().Be(2);
        ((int)CollegeStatus.Suspended).Should().Be(3);
    }

    [Fact]
    public void AddressType_ShouldHaveExpectedValues()
    {
        ((int)AddressType.Permanent).Should().Be(1);
        ((int)AddressType.Temporary).Should().Be(2);
        ((int)AddressType.Current).Should().Be(3);
    }

    [Fact]
    public void LocalLevelType_ShouldHaveExpectedValues()
    {
        ((int)LocalLevelType.RuralMunicipality).Should().Be(1);
        ((int)LocalLevelType.Municipality).Should().Be(2);
        ((int)LocalLevelType.SubMetropolitan).Should().Be(3);
        ((int)LocalLevelType.Metropolitan).Should().Be(4);
    }

    [Fact]
    public void RetotalStatus_ShouldHaveExpectedValues()
    {
        ((int)RetotalStatus.Pending).Should().Be(1);
        ((int)RetotalStatus.UnderReview).Should().Be(2);
        ((int)RetotalStatus.Approved).Should().Be(3);
        ((int)RetotalStatus.Rejected).Should().Be(4);
        ((int)RetotalStatus.Completed).Should().Be(5);
    }

    [Fact]
    public void StudentEnrollmentStatus_ShouldHaveExpectedValues()
    {
        ((int)StudentEnrollmentStatus.Active).Should().Be(1);
        ((int)StudentEnrollmentStatus.Inactive).Should().Be(2);
        ((int)StudentEnrollmentStatus.Dropped).Should().Be(3);
        ((int)StudentEnrollmentStatus.Other).Should().Be(4);
    }
}
